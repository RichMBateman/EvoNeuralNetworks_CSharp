using ShepherdCrook.Library.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShepherdCrook.Library.ANN
{
    public class NetworkMutator
    {
        #region Private Members

        private readonly Network m_network;

        private readonly Dictionary<int, List<int>> m_validSourceToTargets = new Dictionary<int, List<int>>();

        #endregion

        #region Constructors

        public NetworkMutator(Network network)
        {
            m_network = network;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Randomly alters the weight of one of the links.  
        /// </summary>
        public void MutateWeight()
        {
            if (m_network.LinkCount > 0)
            {
                List<Link> allLinks = m_network.MapIdToAllLinks.Values.ToList();
                int linkSelection = RNG.Rnd(allLinks.Count);
                Link selectedLink = allLinks[linkSelection];
                int desiredBehavior = RNG.Rnd(4);
                if (selectedLink.Weight == 0)
                {
                    // if the link is disabled, re-enable it.
                    desiredBehavior = 3; 
                }
                
                switch(desiredBehavior)
                {
                    case 0: selectedLink.Weight *= (1 + RNG.Rnd(0.05, 0.10)); break;
                    case 1: selectedLink.Weight *= (RNG.Rnd(0.90, 0.95)); break;
                    case 2: selectedLink.Weight *= -1; break;
                    case 3: m_network.RandomizeLinkWeight(selectedLink); break;
                }
            }
        }

        /// <summary>
        /// Mutates a new node in the network.  Unlike with new links, it is always possible (and easy) to add a new node.
        /// </summary>
        public void MutateNewNode()
        {
            List<Link> allLinks = new List<Link>(m_network.MapIdToAllLinks.Values);
            int selection = RNG.Rnd(allLinks.Count);
            Link linkToSplit = allLinks[selection];
            Node sourceNode = linkToSplit.NodeIn;
            Node targetNode = linkToSplit.NodeOut;

            Node hiddenNew = m_network.CreateNode();
            hiddenNew.Role = NodeRole.Hidden;
            m_network.AddNode(hiddenNew);
            m_network.CreateNewLinkBetweenExistingNodes(sourceNode, hiddenNew);
            m_network.CreateNewLinkBetweenExistingNodes(hiddenNew, targetNode);

            // Preserve the split link's weight on one of the new links (decided randomly)
            if(RNG.Rnd() < 0.5)
            {
                hiddenNew.LinksIncoming[sourceNode.Id].Weight = linkToSplit.Weight;
            }
            else
            {
                hiddenNew.LinksOutgoing[targetNode.Id].Weight = linkToSplit.Weight;
            }

            // Remove the old link
            sourceNode.LinksOutgoing.Remove(targetNode.Id);
            targetNode.LinksIncoming.Remove(sourceNode.Id);
            m_network.RemoveLink(linkToSplit);

            m_network.VerifyNetworkConnectivity();
        }

        /// <summary>
        /// If possible, inserts a new link between two previously unconnected nodes.
        /// May do nothing if it chooses two already-connected nodes.
        /// </summary>
        public void MutateNewLink()
        {
            List<Node> allNodes = m_network.MapIdToAllNodes.Values.ToList();
            Node sourceNode = allNodes[RNG.Rnd(allNodes.Count)];
            Node targetNode = allNodes[RNG.Rnd(allNodes.Count)];

            if (!m_network.AreNodesConnected(sourceNode, targetNode))
            {
                m_network.CreateNewLinkBetweenExistingNodes(sourceNode, targetNode);
                m_network.VerifyNetworkConnectivity();
            }
        }
        
        /// <summary>
        /// Takes a random link and sets its weight to 0, preventing it from being used.
        /// </summary>
        public void MutateDeleteLink()
        {
            List<Link> allLinks = new List<Link>(m_network.MapIdToAllLinks.Values);
            int selection = RNG.Rnd(allLinks.Count);
            Link selectedLink = allLinks[selection];
            selectedLink.Weight = 0;
        }

        /// <summary>
        /// Deletes a random hidden node.
        /// </summary>
        public void MutateDeleteNode()
        {
            if(m_network.NodesHidden.Count > 0)
            {
                m_network.VerifyNetworkConnectivity();

                Node selectedNode = m_network.NodesHidden[RNG.Rnd(m_network.NodesHidden.Count)];
                List<Link> selectedNodeIncomingLinks = new List<Link>(selectedNode.LinksIncoming.Values);
                List<Link> selectedNodeOutgoingLinks = new List<Link>(selectedNode.LinksOutgoing.Values);

                // Connect every node that feeds into the hidden node with every node this hidden node feeds.
                // This may result in numerous connections being formed.
                foreach (var inNodeLink in selectedNodeIncomingLinks)
                {
                    foreach(var outNodeLink in selectedNodeOutgoingLinks)
                    {
                        Node sourceNode = inNodeLink.NodeIn;
                        Node targetNode = outNodeLink.NodeOut;

                        // Make sure the source node and target node are not also the hidden node.
                        if (sourceNode.Id != selectedNode.Id && targetNode.Id != selectedNode.Id)
                        {
                            if (!m_network.AreNodesConnected(sourceNode, targetNode))
                            {
                                m_network.CreateNewLinkBetweenExistingNodes(sourceNode, targetNode);
                                m_network.VerifyNetworkConnectivity();
                            }

                            // It's possible the links from this hidden node to the other nodes have
                            // already been deleted.  
                            if(selectedNode.LinksIncoming.ContainsKey(sourceNode.Id))
                            {
                                Link originalSourceToHidden = selectedNode.LinksIncoming[sourceNode.Id];
                                m_network.RemoveLink(originalSourceToHidden);
                                selectedNode.LinksIncoming.Remove(sourceNode.Id);
                            }
                            if (selectedNode.LinksOutgoing.ContainsKey(targetNode.Id))
                            {
                                Link originalHiddenToTarget = selectedNode.LinksOutgoing[targetNode.Id];
                                m_network.RemoveLink(originalHiddenToTarget);
                                selectedNode.LinksOutgoing.Remove(targetNode.Id);
                            }

                            sourceNode.LinksOutgoing.Remove(selectedNode.Id);
                            targetNode.LinksIncoming.Remove(selectedNode.Id);

                            m_network.VerifyNetworkConnectivity();
                        }
                        else
                        {
                            // The hidden node that we are removing is pointing to itself.  We can safely remove the link.
                            if (sourceNode.Id == selectedNode.Id && targetNode.Id == selectedNode.Id)
                            {
                                selectedNode.LinksIncoming.Remove(selectedNode.Id);
                                selectedNode.LinksOutgoing.Remove(selectedNode.Id);
                                m_network.RemoveLink(inNodeLink);
                            }
                        }
                    }
                }

                // The selected Node should have no links at this point.
                if (selectedNode.LinksIncoming.Count > 0) throw new Exception();
                if (selectedNode.LinksOutgoing.Count > 0) throw new Exception();

                // At this point, we can remove the hidden node.  
                // Any links it used to refer to should have already been removed.
                m_network.RemoveHiddenNode(selectedNode);
                m_network.VerifyNetworkConnectivity();
            }
        }

        #endregion

    }
}
