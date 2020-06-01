namespace Uneti.NET
{
    /// <summary>
    /// Represents a pair of similar <see cref="Node"/>.
    /// </summary>
    internal sealed class NodePair
    {
        /// <summary>
        /// Creates a new instance of <see cref="NodePair"/>.
        /// </summary>
        /// <param name="actualNode">The actual node.</param>
        /// <param name="expectedNode">The expected node.</param>
        /// <param name="nodeScore">The node similarity score.</param>
        /// <param name="childrenScore">The node children similarity score.</param>
        /// <param name="siblingsScore">The node siblings similarity score.</param>
        internal NodePair(Node actualNode, Node expectedNode, float nodeScore, float childrenScore, float siblingsScore)
        {
            AverageScore = (nodeScore + childrenScore + siblingsScore) / 3.0f;
            ActualNode = actualNode;
            ExpectedNode = expectedNode;
            NodeScore = nodeScore;
        }

        /// <summary>
        /// The average similarity score between the node, children and siblings scores for the current node pair.
        /// </summary>
        internal float AverageScore { get; }
        
        /// <summary>
        /// The actual node for the current node pair.
        /// </summary>
        internal Node ActualNode { get; }
        
        /// <summary>
        /// The expected node for the current node pair.
        /// </summary>
        internal Node ExpectedNode { get; }
        
        /// <summary>
        /// The node score for the current node pair.
        /// </summary>
        internal float NodeScore { get; }

        /// <summary>
        /// Tries to match the <see cref="ActualNode"/> to the <see cref="ExpectedNode"/>.
        /// </summary>
        /// <returns>True if the <see cref="ActualNode"/> can match to the <see cref="ExpectedNode"/>; otherwise, false.</returns>
        internal bool TryCreateMatch()
        {
            return ActualNode.TryMatch(ExpectedNode);
        }
    }
}