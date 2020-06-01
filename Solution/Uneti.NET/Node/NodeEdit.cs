using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml.Linq;

namespace Uneti.NET
{
    /// <summary>
    /// Represents an edit between two <see cref="Node"/>.
    /// </summary>
    internal sealed class NodeEdit
    {
        /// <summary>
        /// Creates a new instance of <see cref="NodeEdit"/>.
        /// </summary>
        /// <param name="actualNode">The actual node.</param>
        /// <param name="expectedNode">The expected node.</param>
        /// <param name="operation">The edit operation.</param>
        private NodeEdit(Node actualNode, Node expectedNode, Operation operation)
        {
            ActualNode = actualNode;
            ExpectedNode = expectedNode;
            Operation = operation;
        }

        /// <summary>
        /// The actual node of the current edit.
        /// </summary>
        internal Node ActualNode { get; }
        
        /// <summary>
        /// The expected node of the current edit.
        /// </summary>
        internal Node ExpectedNode { get; }
    
        /// <summary>
        /// The operation of the current edit.
        /// </summary>
        internal Operation Operation { get; }
        
        /// <summary>
        /// Gets the minimal set of edits needed to transform the specified expected text into the specified actual text.
        /// </summary>
        /// <param name="expectedText">The expected text.</param>
        /// <param name="actualText">The actual text.</param>
        /// <param name="excludeEmptyNodes">Should empty nodes be excluded from the results?</param>
        /// <param name="predicate">If true, the <see cref="XElement"/> will not be loaded.</param>
        /// <returns>The minimal set of edits needed to transform the specified expected text into the specified actual text.</returns>
        internal static IEnumerable<NodeEdit> GetEdits(string expectedText, string actualText, bool excludeEmptyNodes, Func<XElement, bool> predicate)
        {
            var schemaRegistry = new SchemaRegistry();
            var expectedNodeInfo = NodeInfo.Parse(expectedText, predicate, schemaRegistry);
            var actualNodeInfo = NodeInfo.Parse(actualText, predicate, schemaRegistry);
            var expectedNodeGroups = Node.CreateGroups(expectedNodeInfo, schemaRegistry, out var expectedNodeCount);
            var actualNodeGroups = Node.CreateGroups(actualNodeInfo, schemaRegistry, out var actualNodeCount);
            var nodeComparer = new NodeComparer(expectedNodeCount, actualNodeCount);
            var edits = new List<NodeEdit>();

            foreach (var key in expectedNodeGroups.Keys.Union(actualNodeGroups.Keys))
            {
                if (expectedNodeGroups.TryGetValue(key, out var expectedNodes))
                {
                    edits.AddRange(actualNodeGroups.TryGetValue(key, out var actualNodes) ? GetEdits(expectedNodes, actualNodes, nodeComparer, excludeEmptyNodes) : expectedNodes.Where(node => !excludeEmptyNodes || !node.IsEmpty).Select(node => new NodeEdit(null, node, Operation.Removed)));
                }
                else if (actualNodeGroups.TryGetValue(key, out var actualNodes))
                {
                    edits.AddRange(actualNodes.Where(node => !excludeEmptyNodes || !node.IsEmpty).Select(node => new NodeEdit(node, null, Operation.Added)));
                }
            }

            return edits;
        }

        /// <summary>
        /// Gets the minimal set of edits needed to transform the specified expected nodes into the specified actual nodes.
        /// </summary>
        /// <param name="expectedNodes">The expected nodes.</param>
        /// <param name="actualNodes">The actual nodes.</param>
        /// <param name="nodeComparer">The comparer used to determine node similarity.</param>
        /// <param name="excludeEmptyNodes">Should empty nodes be excluded from the results?</param>
        /// <returns>A new <see cref="NodeEdit"/> for each <see cref="NodePair"/> where the nodes are not identical.</returns>
        private static IEnumerable<NodeEdit> GetEdits(ImmutableArray<Node> expectedNodes, ImmutableArray<Node> actualNodes, NodeComparer nodeComparer, bool excludeEmptyNodes)
        {
            const float epsilon = 0.00001f;

            var count = 0;
            var maximum = Math.Min(expectedNodes.Length, actualNodes.Length);

            foreach (var nodePair in nodeComparer.GetNodePairs(expectedNodes, actualNodes).OrderByDescending(nodePair => nodePair.AverageScore))
            {
                if (!nodePair.TryCreateMatch()) continue;

                if (Math.Abs(nodePair.NodeScore - 1.0f) > epsilon)
                {
                    yield return new NodeEdit(nodePair.ActualNode, nodePair.ExpectedNode, Operation.Modified);
                }

                if (maximum == ++count) break;
            }

            foreach (var expectedNode in expectedNodes.Where(expectedNode => !expectedNode.IsMatched && (!excludeEmptyNodes || !expectedNode.IsEmpty)))
            {
                yield return new NodeEdit(null, expectedNode, Operation.Removed);
            }

            foreach (var actualNode in actualNodes.Where(actualNode => !actualNode.IsMatched && (!excludeEmptyNodes || !actualNode.IsEmpty)))
            {
                yield return new NodeEdit(actualNode, null, Operation.Added);
            }
        }
    }
}