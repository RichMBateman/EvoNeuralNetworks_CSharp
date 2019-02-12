using System.Collections.Generic;

namespace ShepherdCrook.Library.ANN
{
    /// <summary>
    /// Describes how this node functions within the network.
    /// (Input, Bias, Hidden, Output)
    /// </summary>
    public enum NodeRole
    {
        Input,
        Bias,
        Hidden,
        Output,
    }
    public class Node
    {
        #region Private Members

        private readonly Dictionary<int, Link> m_linksIncoming = new Dictionary<int, Link>();
        private readonly Dictionary<int, Link> m_linksOutgoing = new Dictionary<int, Link>();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a node with the specified id.
        /// </summary>
        public Node(int id)
        {
            Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A unique id for this node across the entire application.  Meant for debugging purposes.  Meant to distinguish
        /// this object from others.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// The function this neuron plays in the network.
        /// </summary>
        public NodeRole Role { get; set; }
        /// <summary>
        /// All links coming into this node.  The key is the source node.
        /// </summary>
        public Dictionary<int, Link> LinksIncoming { get { return m_linksIncoming; } }
        /// <summary>
        /// All links coming out of this node.  The key is the target node.
        /// </summary>
        public Dictionary<int, Link> LinksOutgoing { get { return m_linksOutgoing; } }
        /// <summary>
        /// For all nodes that feed into this node, the output of that node * the weight of the link to this node.
        /// That is summed across all incoming nodes.
        /// </summary>
        public double IncomingActivity { get; set; }
        /// <summary>
        /// The current activation.
        /// </summary>
        public double ActivationCurrent { get; set; }
        /// <summary>
        /// The previous activation
        /// </summary>
        public double ActivationPrevious { get; set; }

        #endregion

        #region Public Methods

        #region Queries

        /// <summary>
        /// Makes a copy of the Node, but clears any activation.  Includes original links to be deep copied.
        /// Copies the id as well.
        /// </summary>
        public Node Copy()
        {
            Node copy = new Node(Id);
            copy.Role = Role;
            copy.IncomingActivity = 0;
            copy.ActivationCurrent = 0;
            copy.ActivationPrevious = 0;

            foreach(var kvp in LinksIncoming)
            {
                copy.LinksIncoming.Add(kvp.Key, kvp.Value);
            }
            foreach (var kvp in LinksOutgoing)
            {
                copy.LinksOutgoing.Add(kvp.Key, kvp.Value);
            }
            return copy;
        }

        public override string ToString()
        {
            return ANNIO.ToStringRepresentation(this);
        }

        #endregion

        #region Updates

        #endregion

        #endregion
    }
}
