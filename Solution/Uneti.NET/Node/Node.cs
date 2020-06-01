using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Uneti.NET
{
    /// <summary>
    /// Represents the data at a specific location within an XML tree.
    /// </summary>
    internal sealed class Node
    {
        /// <summary>
        /// Creates a new instance of <see cref="Node"/>.
        /// </summary>
        /// <param name="nodeInfo">The information needed to create a new node.</param>
        /// <param name="parent">The parent to the created node.</param>
        /// <param name="counter">Sets the index of the node.</param>
        /// <param name="schemaRegistry">Stores the schema of the node.</param>
        private Node(NodeInfo nodeInfo, Node parent, Counter counter, SchemaRegistry schemaRegistry)
        {
            Children = nodeInfo.Children.Select(childNodeInfo => new Node(childNodeInfo, this, counter, schemaRegistry)).ToImmutableArray();
            Element = nodeInfo.Element;
            Index = counter.Next();
            Parent = parent;
            Properties = schemaRegistry.Format(nodeInfo.Signature, nodeInfo.Properties, out var schemaId);
            SchemaId = schemaId;
        }

        /// <summary>
        /// The nodes underneath and directly connected to the current node.
        /// </summary>
        internal ImmutableArray<Node> Children { get; }

        /// <summary>
        /// The XML the current node represents.
        /// </summary>
        internal XElement Element { get; }

        /// <summary>
        /// The index of the current node in contiguous memory.
        /// </summary>
        internal int Index { get; }

        /// <summary>
        /// Is the current node empty? (i.e. there is no information stored in the current node)
        /// </summary>
        internal bool IsEmpty => Properties.IsEmpty || Properties.All(property => property.IsEmpty);

        /// <summary>
        /// Has the current node been matched to another node?
        /// </summary>
        internal bool IsMatched { get; private set; }

        /// <summary>
        /// The node above and directly connected to the current node.
        /// </summary>
        internal Node Parent { get; }

        /// <summary>
        /// The properties of the current node.
        /// </summary>
        private ImmutableArray<Bigram> Properties { get; }

        /// <summary>
        /// The schema ID of the current node.
        /// </summary>
        private int SchemaId { get; }

        /// <summary>
        /// Creates node groups based upon their schema from the specified node inforation.
        /// </summary>
        /// <param name="nodeInfo">The node information for the root node.</param>
        /// <param name="schemaRegistry">Stores the schemas of all created nodes.</param>
        /// <param name="count">The total number of nodes created.</param>
        /// <returns>Nodes grouped by their schema ID.</returns>
        internal static ImmutableDictionary<int, ImmutableArray<Node>> CreateGroups(NodeInfo nodeInfo, SchemaRegistry schemaRegistry, out int count)
        {
            var counter = new Counter();
            var root = new Node(nodeInfo, null, counter, schemaRegistry);
            var nodes = new Node[count = counter.Current()];
            var stack = new Stack<Node>();

            stack.Push(root);

            while (stack.TryPop(out var node))
            {
                nodes[node.Index] = node;

                foreach (var child in node.Children)
                {
                    stack.Push(child);
                }
            }

            return nodes.GroupBy(node => node.SchemaId).ToImmutableDictionary(grouping => grouping.Key, grouping => grouping.ToImmutableArray());
        }

        /// <summary>
        /// Calculates the similarity of the current node to the specified node.
        /// </summary>
        /// <param name="other">The node to compare the current node against.</param>
        /// <returns>A value between 0 and 1, where 1 is a perfect match.</returns>
        internal float CompareTo(Node other)
        {
            if (SchemaId != other.SchemaId) return 0.0f;
            if (Properties.IsEmpty && other.Properties.IsEmpty) return 1.0f;

            return Properties.Select((bigram, index) => bigram.CompareTo(other.Properties[index])).Average();
        }

        /// <summary>
        /// Tries to match the current node to the specified node.
        /// </summary>
        /// <param name="other">The node to match to the current node.</param>
        /// <returns>True if the match was successful; otherwise, false.</returns>
        internal bool TryMatch(Node other)
        {
            return !IsMatched && !other.IsMatched && (IsMatched = other.IsMatched = true);
        }
    }
}