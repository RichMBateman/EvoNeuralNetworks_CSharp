using ShepherdCrook.Library.Math;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShepherdCrook.Library.ANN
{
    public class Network
    {
        #region Constants and Static Members

        public const string NodeNamePrefixInput = "Input";
        public const string NodeNamePrefixBias = "Bias";
        public const string NodeNamePrefixHidden = "Hidden";
        public const string NodeNamePrefixOutput = "Output";

        public const double LinkWeightInitMin = -1;
        public const double LinkWeightInitMax = +1;
        private static int NetworkUniqueIdGenerator = 0;

        #endregion

        #region Private Members

        /// <summary>
        /// A dictionary of all nodes, based on Id.
        /// </summary>
        private readonly Dictionary<int, Node> m_mapIdToAllNodes = new Dictionary<int, Node>();
        /// <summary>
        /// A list of input nodes, excluding the bias.  This list is only populated once, and its contents never change.
        /// </summary>
        private readonly List<Node> m_nodesInput = new List<Node>();
        /// <summary>
        /// The one bias node that will ever exists.  If the input vector presented to the network is all 0, the bias neuron gives
        /// any output or hidden neurons a chance to activate.
        /// </summary>
        private Node m_nodeBias;
        /// <summary>
        /// A list of input nodes and the bias.  Only populated once.
        /// </summary>
        private readonly List<Node> m_nodesInputAndBias = new List<Node>();
        private readonly List<Node> m_nodesHidden = new List<Node>();
        /// <summary>
        /// A list of output nodes.  This list is only populated once, and its contents never change.
        /// </summary>
        private readonly List<Node> m_nodesOutput = new List<Node>();
        /// <summary>
        /// All links in the system.
        /// </summary>
        private readonly Dictionary<int, Link> m_mapIdToAllLinks = new Dictionary<int, Link>();
        /// <summary>
        /// Capable of mutating this network.
        /// </summary>
        private NetworkMutator m_mutator;
        private int m_idGeneratorNode;
        private int m_idGeneratorLink;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty network.
        /// </summary>
        public Network()
        {
            Id = NetworkUniqueIdGenerator;
            NetworkUniqueIdGenerator++;
        }

        /// <summary>
        /// Creates a network from a string representation
        /// </summary>
        public Network(List<String> lines)
        {
            ANNIO.FromStringRepresentation(this, lines);
            NetworkUniqueIdGenerator++;
        }

        /// <summary>
        /// Creates a fully-connected, minimal network with a number of input and output neurons.
        /// </summary>
        public Network(int numInputs, int numOutputs) : this()
        {
            CreateMinimalNetwork(numInputs, numOutputs);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A unique id number for this neural network, to help distinguish it from all other networks.
        /// </summary>
        public int Id { get; internal set; }
        /// <summary>
        /// Dictionary of all nodes (mapped by id)
        /// </summary>
        public Dictionary<int, Node> MapIdToAllNodes
        {
            get
            {
                return m_mapIdToAllNodes;
            }
        }
        /// <summary>
        /// Dictionary of all links (mapped by id)
        /// </summary>
        public Dictionary<int, Link> MapIdToAllLinks
        {
            get
            {
                return m_mapIdToAllLinks;
            }
        }
        /// <summary>
        /// A list of the output nodes for this network.
        /// </summary>
        public List<Node> NodesOutput { get { return m_nodesOutput; } }
        /// <summary>
        /// Hidden nodes
        /// </summary>
        public List<Node> NodesHidden { get { return m_nodesHidden; } }
        /// <summary>
        /// Input nodes (including bias)
        /// </summary>
        public List<Node> NodesInputAndBias { get { return m_nodesInputAndBias; } }
        /// <summary>
        /// The total count of all nodes.
        /// </summary>
        public Int32 NodeCount
        {
            get { return m_mapIdToAllNodes.Values.Count; }
        }
        /// <summary>
        /// The total count of all links.
        /// </summary>
        public Int32 LinkCount
        {
            get { return m_mapIdToAllLinks.Values.Count; }
        }
        /// <summary>
        /// A module capable of mutating this network.
        /// </summary>
        public NetworkMutator Mutator
        {
            get
            {
                if(m_mutator == null)
                {
                    m_mutator = new NetworkMutator(this);
                }
                return m_mutator;
            }
        }

        #endregion

        #region Public Methods

        #region Initialization

        /// <summary>
        /// Adds this node to the network.
        /// </summary>
        public void AddNode(Node n)
        {
            m_mapIdToAllNodes.Add(n.Id, n);
            switch(n.Role)
            {
                case NodeRole.Input:
                    m_nodesInput.Add(n);
                    m_nodesInputAndBias.Add(n);
                    break;
                case NodeRole.Bias:
                    m_nodeBias = n;
                    m_nodesInputAndBias.Add(n);
                    n.ActivationCurrent = 1.0;
                    n.ActivationPrevious = 1.0;
                    break;
                case NodeRole.Hidden:
                    m_nodesHidden.Add(n);
                    break;
                case NodeRole.Output:
                    m_nodesOutput.Add(n);
                    break;
            }
        }

        /// <summary>
        /// Adds this network to the network and hooks up the nodes to it.
        /// </summary>
        public void AddLink(Link link)
        {
            m_mapIdToAllLinks.Add(link.Id, link);
            MapIdToAllNodes[link.NodeIn.Id].LinksOutgoing.Add(link.NodeOut.Id, link);
            MapIdToAllNodes[link.NodeOut.Id].LinksIncoming.Add(link.NodeIn.Id, link);
        }

        /// <summary>
        /// Removes a hidden node from the network.
        /// </summary>
        public void RemoveHiddenNode(Node n)
        {
            m_mapIdToAllNodes.Remove(n.Id);
            m_nodesHidden.Remove(n);
        }

        /// <summary>
        /// Removes this link from the network.  Assumes no node refers to this link anymore.
        /// </summary>
        public void RemoveLink(Link link)
        {
            m_mapIdToAllLinks.Remove(link.Id);
        }

        #endregion

        #region Activation

        /// <summary>
        /// Computes the activation for the given input vector.
        /// </summary>
        /// <param name="inputVector">Ordered array of inputs.  (Bias is always 1)</param>
        public List<double> ComputeActivation(List<double> inputVector)
        {
            // First, update the activation of all input neurons, remembering the previous activation.
            UpdateInputLayerActivation(inputVector);

            // Figure out activation of output layer by recursively figuring out output of all inputs, 
            // one level at a time.  If we have to ask for the activation of a neuron more than once,
            // we use the previous activation (must have a recurrent link)
            foreach (Node outputNode in m_nodesOutput)
            {
                List<Node> previousNodeRequests = new List<Node>();
                ComputeActivation(outputNode, previousNodeRequests);
            }

            List<double> activations = new List<double>();
            for (int o = 0; o < m_nodesOutput.Count; o++)
            {
                activations.Add(m_nodesOutput[o].ActivationCurrent);
            }
            return activations;
        }

        #endregion

        #region Queries

        /// <summary>
        /// Iterates through all nodes in the network, looking for errors.
        /// All nodes and links should make sense.
        /// </summary>
        public void VerifyNetworkConnectivity()
        {
            // iterate over all known nodes and check connectivity.
            // Each pair of nodes better agree they share the same state of connectivity.
            foreach(Node nSrc in m_mapIdToAllNodes.Values)
            {
                foreach (Node nTgt in m_mapIdToAllNodes.Values)
                {
                    AreNodesConnected(nSrc, nTgt);
                }
            }

            // Iterate over all links, looking for problems.
            foreach(Link l in m_mapIdToAllLinks.Values)
            {
                // We better know about all nodes each link is referring to
                if (!m_mapIdToAllNodes.ContainsKey(l.NodeIn.Id)) throw new Exception();
                if (!m_mapIdToAllNodes.ContainsKey(l.NodeOut.Id)) throw new Exception();
                // If the nodes in the link are not actually connected, that's a problem.
                if (!AreNodesConnected(l.NodeIn, l.NodeOut)) throw new Exception();
            }

            // Count all the links (each link should be counted twice).  Should match the links we know about.
            int linkCount = 0;
            foreach(Node n in m_mapIdToAllNodes.Values)
            {
                linkCount += n.LinksIncoming.Count;
                linkCount += n.LinksOutgoing.Count;
            }
            if(linkCount != m_mapIdToAllLinks.Count * 2)
            {
                throw new Exception();
            }
        }

        /// <summary>
        /// Returns a deep copy of this network, with identical weights and id numbers.
        /// </summary>
        public Network Copy()
        {
            Network copyNetwork = new Network();
            copyNetwork.m_idGeneratorLink = m_idGeneratorLink;
            copyNetwork.m_idGeneratorNode = m_idGeneratorNode;
            CopyNodesToNetwork(copyNetwork);
            CopyLinksToNetwork(copyNetwork);
            CopyNetworkUpdateNodeShallowLinks(copyNetwork);

            copyNetwork.VerifyNetworkConnectivity();

            return copyNetwork;
        }

        /// <summary>
        /// A string representation of this node.
        /// </summary>
        public override string ToString()
        {
            return ANNIO.ToStringRepresentation(this);
        }

        #endregion

        #region Updates

        /// <summary>
        /// Randomizes all weights on the network.
        /// </summary>
        public void RandomizeWeights()
        {
            foreach (Link l in m_mapIdToAllLinks.Values)
            {
                RandomizeLinkWeight(l);
            }
        }

        #endregion

        #endregion

        #region Private Methods

        #region Initialization

        private void CreateMinimalNetwork(int numInputs, int numOutputs)
        {
            CreateMinimalNetworkNodes(numInputs, numOutputs);
            CreateMinimalNetworkLinks();
        }

        private void CreateMinimalNetworkNodes(int numInputs, int numOutputs)
        {
            m_nodeBias = CreateNode();
            m_nodeBias.Role = NodeRole.Bias;
            AddNode(m_nodeBias);

            for (int i = 0; i < numInputs; i++)
            {
                Node input = CreateNode();
                input.Role = NodeRole.Input;
                AddNode(input);
            }

            for (int i = 0; i < numOutputs; i++)
            {
                Node output = CreateNode();
                output.Role = NodeRole.Output;
                AddNode(output);
            }
        }

        private void CreateMinimalNetworkLinks()
        {
            foreach (Node input in m_nodesInputAndBias)
            {
                foreach (Node output in m_nodesOutput)
                {
                    CreateNewLinkBetweenExistingNodes(input, output);
                }
            }
        }

        #endregion

        #region Activation

        /// <summary>
        /// Updates the ActivationPrevious with the Current activation,
        /// and sees the ActivationCurrent to what's supplied from input vector.
        /// The bias neuron is always 1 and doesn't need to change.
        /// </summary>
        private void UpdateInputLayerActivation(List<double> inputVector)
        {
            for (int i = 0; i < m_nodesInput.Count; i++)
            {
                m_nodesInput[i].ActivationPrevious = m_nodesInput[i].ActivationCurrent;
                m_nodesInput[i].ActivationCurrent = inputVector[i];
            }
            // The bias node is permanently set to 1, so no need to set again.
        }

        /// <summary>
        /// Computes the activation of node n.  We initially start with output neurons, and work
        /// our way backward.
        /// </summary>
        private void ComputeActivation(Node n, List<Node> previousNodeRequests)
        {
            if (n.Role == NodeRole.Input || n.Role == NodeRole.Bias)
            {
                // It doesn't make sense to compute the activation of Input and Bias nodes.
                // The activation of the Bias node is always 1
                // The activation of the input node comes from an external input vector.
                return;
            }

            n.IncomingActivity = 0;
            foreach (Link l in n.LinksIncoming.Values)
            {
                double activationContribution = 0;
                if(previousNodeRequests.Contains(l.NodeIn))
                {
                    // We've already asked about this node, so we must have hit a recurrent loop.
                    // Use the previous activation, and stop asking.
                    activationContribution = l.NodeIn.ActivationPrevious * l.Weight;
                }
                else
                {
                    previousNodeRequests.Add(l.NodeIn);
                    ComputeActivation(l.NodeIn, previousNodeRequests);
                    // After computing, pop the last item off the list
                    previousNodeRequests.RemoveAt(previousNodeRequests.Count - 1);
                    activationContribution = l.NodeIn.ActivationCurrent * l.Weight;
                }
                n.IncomingActivity += activationContribution;
            }

            n.ActivationPrevious = n.ActivationCurrent;
            n.ActivationCurrent = ActivationFunctions.Sigmoidal_0_1(n.IncomingActivity);
        }

        #endregion

        #region Copy

        private void CopyNodesToNetwork(Network other)
        {
            foreach (Node n in m_mapIdToAllNodes.Values)
            {
                Node copyNode = n.Copy();
                switch (copyNode.Role)
                {
                    case NodeRole.Input:
                        other.m_nodesInput.Add(copyNode);
                        other.m_nodesInputAndBias.Add(copyNode);
                        break;
                    case NodeRole.Bias:
                        other.m_nodeBias = copyNode;
                        other.m_nodesInputAndBias.Add(copyNode);
                        copyNode.ActivationCurrent = 1;
                        copyNode.ActivationPrevious = 1;
                        break;
                    case NodeRole.Hidden:
                        other.m_nodesHidden.Add(copyNode);
                        break;
                    case NodeRole.Output:
                        other.m_nodesOutput.Add(copyNode);
                        break;
                }
                other.m_mapIdToAllNodes.Add(copyNode.Id, copyNode);
            }
        }

        /// <summary>
        /// Copies the link from the other network, and replaces shallow node copies with deep ones.
        /// </summary>
        private void CopyLinksToNetwork(Network other)
        {
            foreach (Link l in m_mapIdToAllLinks.Values)
            {
                Link copyLink = l.Copy();
                other.m_mapIdToAllLinks.Add(copyLink.Id, copyLink);
                copyLink.NodeIn = other.m_mapIdToAllNodes[copyLink.NodeIn.Id];
                copyLink.NodeOut = other.m_mapIdToAllNodes[copyLink.NodeOut.Id];
            }
        }

        private void CopyNetworkUpdateNodeShallowLinks(Network other)
        {
            foreach (Node n in other.m_mapIdToAllNodes.Values)
            {
                List<Link> lDeepIncoming = new List<Link>();
                List<Link> lDeepOutgoing = new List<Link>();
                foreach (Link l in n.LinksIncoming.Values)
                {
                    lDeepIncoming.Add(other.MapIdToAllLinks[l.Id]);
                }
                foreach (Link l in n.LinksOutgoing.Values)
                {
                    lDeepOutgoing.Add(other.MapIdToAllLinks[l.Id]);
                }
                // Remove shallow copies
                n.LinksIncoming.Clear();
                n.LinksOutgoing.Clear();

                // Add deep copies
                foreach(Link l in lDeepIncoming)
                {
                    n.LinksIncoming.Add(l.NodeIn.Id, l);
                }
                foreach (Link l in lDeepOutgoing)
                {
                    n.LinksOutgoing.Add(l.NodeOut.Id, l);
                }
            }
        }

        #endregion

        #region Nodes

        /// <summary>
        /// Creates a new node with a new id unique to this network.
        /// </summary>
        public Node CreateNode()
        {
            Node n = new Node(m_idGeneratorNode++);
            return n;
        }

        #endregion

        #region Links

        public Link CreateLink()
        {
            Link l = new Link(m_idGeneratorLink++);
            return l;
        }

        public void RandomizeLinkWeight(Link l)
        {
            l.Weight = RNG.Rnd(LinkWeightInitMin, LinkWeightInitMax);
        }

        /// <summary>
        /// Returns true if the nodes are connected.
        /// Will throw an exception if one node thinks it's connected to the other, but not vice versa.
        /// </summary>
        public bool AreNodesConnected(Node sourceNode, Node targetNode)
        {
            bool nodesConnected = false;

            bool srcConnectsToTgt = sourceNode.LinksOutgoing.ContainsKey(targetNode.Id);
            bool tgtConnectsToSrc = targetNode.LinksIncoming.ContainsKey(sourceNode.Id);

            nodesConnected = (srcConnectsToTgt && tgtConnectsToSrc);
            if(srcConnectsToTgt ^ tgtConnectsToSrc)
            {
                throw new Exception("Node connection mismatch.");
            }
            return nodesConnected;
        }

        #endregion

        #region Topology Manipulation

        public Link CreateNewLinkBetweenExistingNodes(Node source, Node target)
        {
            Link link = CreateLink();
            link.NodeIn = source;
            link.NodeOut = target;
            RandomizeLinkWeight(link);

            m_mapIdToAllLinks.Add(link.Id, link);
            source.LinksOutgoing.Add(target.Id, link);
            target.LinksIncoming.Add(source.Id, link);

            return link;
        }

        #endregion

        #endregion
    }
}
