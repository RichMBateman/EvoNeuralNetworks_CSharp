namespace ShepherdCrook.Library.ANN
{
    /// <summary>
    /// Describes a unidirectional connection between two nodes.
    /// </summary>
    public class Link
    {
        #region Constructors

        /// <summary>
        /// Creates a new link with the supplied id.
        /// </summary>
        public Link(int id)
        {
            Id = id;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// A unique id for this link across the entire application.  Meant for debugging purposes, and to identify this object
        /// amongst others.
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// All links are directed.  This represents the node that feeds into this link.  
        /// (i.e., this node's output value will multiply by this link's weight, and will form part of the input of NodeOut)
        /// </summary>
        public Node NodeIn { get; set; }
        /// <summary>
        /// All links are directed.  This represents the node that this link feeds into.
        /// (i.e., this node's input will represent the sum of the output of all incoming links)
        /// </summary>
        public Node NodeOut { get; set; }
        /// <summary>
        /// The weight of this link
        /// </summary>
        public double Weight { get; set; }

        #endregion

        #region Public Methods

        #region Queries

        /// <summary>
        /// Returns a copy of the link.  Shallow copies of the Node In and Out, however.
        /// Copies the id as well.
        /// </summary>
        public Link Copy()
        {
            Link copy = new Link(Id);

            copy.NodeIn = NodeIn;
            copy.NodeOut = NodeOut;
            copy.Weight = Weight;

            return copy;
        }

        public override string ToString()
        {
            return ANNIO.ToStringRepresentation(this);
        }

        #endregion

        #endregion
    }
}
