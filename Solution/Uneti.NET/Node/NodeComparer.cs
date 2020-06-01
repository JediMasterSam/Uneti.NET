using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Uneti.NET
{
    /// <summary>
    /// Used for comparing <see cref="Node"/>.
    /// </summary>
    internal sealed class NodeComparer
    {
        /// <summary>
        /// The threshold that must be met for a direct match. (i.e. are these two nodes the same?)
        /// </summary>
        private const float Threshold1 = 0.6f;

        /// <summary>
        /// The threshold that must be met for a relative match. (i.e. are these two nodes in the same location?)
        /// </summary>
        private const float Threshold2 = 0.8f;

        /// <summary>
        /// Creates a new instance of <see cref="NodeComparer"/>.
        /// </summary>
        /// <param name="expectedNodeCount">The number of expected nodes.</param>
        /// <param name="actualNodeCount">The number of actual nodes.</param>
        internal NodeComparer(int expectedNodeCount, int actualNodeCount)
        {
            ChildrenScores = new float?[expectedNodeCount][];
            NodeScores = new float?[expectedNodeCount][];

            for (var index = 0; index < expectedNodeCount; index++)
            {
                ChildrenScores[index] = new float?[actualNodeCount];
                NodeScores[index] = new float?[actualNodeCount];
            }
        }

        /// <summary>
        /// The scores assigned to node children similarity.
        /// </summary>
        private float?[][] ChildrenScores { get; }

        /// <summary>
        /// The scores assigned to node similarity.
        /// </summary>
        private float?[][] NodeScores { get; }

        /// <summary>
        /// Iterates over both sets of nodes and creates pairs of similar nodes.
        /// </summary>
        /// <param name="expectedNodes">The expected nodes.</param>
        /// <param name="actualNodes">The actual nodes.</param>
        /// <returns>A new instance of <see cref="NodePair"/> for each pair of similar nodes.</returns>
        public IEnumerable<NodePair> GetNodePairs(ImmutableArray<Node> expectedNodes, ImmutableArray<Node> actualNodes)
        {
            foreach (var expectedNode in expectedNodes)
            {
                foreach (var actualNode in actualNodes)
                {
                    if (AreSimilar(expectedNode, actualNode, out var nodeScore, out var childrenScore, out var siblingsScore))
                    {
                        yield return new NodePair(actualNode, expectedNode, nodeScore, childrenScore, siblingsScore);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the two specified nodes are similar.
        /// </summary>
        /// <param name="expectedNode">The expected node.</param>
        /// <param name="actualNode">The actual node.</param>
        /// <param name="nodeScore">The similarity score for the specified nodes.</param>
        /// <param name="childrenScore">The similarity score for the the specified nodes' children.</param>
        /// <param name="siblingsScore">The similarity score for the specified nodes' siblings.</param>
        /// <returns>True if the specified nodes are similar; otherwise, false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private bool AreSimilar(Node expectedNode, Node actualNode, out float nodeScore, out float childrenScore, out float siblingsScore)
        {
            nodeScore = GetNodeScore(expectedNode, actualNode);
            childrenScore = GetChildrenScore(expectedNode, actualNode);

            if (expectedNode.Parent == null && actualNode.Parent == null)
            {
                siblingsScore = 1.0f;
            }
            else if (expectedNode.Parent == null || actualNode.Parent == null)
            {
                siblingsScore = 0.0f;
            }
            else
            {
                siblingsScore = GetChildrenScore(expectedNode.Parent, actualNode.Parent);
            }

            return nodeScore > Threshold1 && childrenScore > Threshold1 || childrenScore > Threshold2 || siblingsScore > Threshold2;
        }

        private float CompareChildren(Node expectedNode, Node actualNode)
        {
            if (expectedNode.Children.IsEmpty && actualNode.Children.IsEmpty) return 1.0f;
            if (expectedNode.Children.IsEmpty || actualNode.Children.IsEmpty) return 0.0f;

            return (float) CountMatches(expectedNode.Children, actualNode.Children) / Math.Max(expectedNode.Children.Length, actualNode.Children.Length);
        }

        /// <summary>
        /// Counts the maximum number of similar nodes between the two specified sets of nodes.
        /// </summary>
        /// <param name="expectedNodes">The expected nodes.</param>
        /// <param name="actualNodes">The actual nodes.</param>
        /// <returns>The maximum number of similar nodes between the two specified sets of nodes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private int CountMatches(ImmutableArray<Node> expectedNodes, ImmutableArray<Node> actualNodes)
        {
            const int invalid = -1;

            var count = 0;
            var m = expectedNodes.Length;
            var n = actualNodes.Length;
            var matches = ArrayPool<int>.Shared.Rent(n);
            var stacks = ArrayPool<Stack<int>>.Shared.Rent(m);

            for (var index = 0; index < n; index++)
            {
                matches[index] = invalid;
            }

            for (var x = 0; x < m; x++)
            {
                var stack = stacks[x] ??= new Stack<int>(n);

                stack.Clear();

                for (var y = 0; y < n; y++)
                {
                    if (GetNodeScore(expectedNodes[x], actualNodes[y]) > Threshold1)
                    {
                        stack.Push(y);
                    }
                }

                var currentX = x;

                while (stack.TryPop(out var y))
                {
                    var nextX = matches[y];

                    if (nextX == invalid)
                    {
                        matches[y] = currentX;
                        count++;
                        break;
                    }

                    var nextStack = stacks[nextX];

                    if (nextStack.Count == 0) break;

                    matches[y] = currentX;
                    currentX = nextX;
                    stack = nextStack;
                }
            }

            ArrayPool<int>.Shared.Return(matches);
            ArrayPool<Stack<int>>.Shared.Return(stacks);

            return count;
        }

        /// <summary>
        /// Gets the score assigned to node children similarity for the specified nodes.  The score is calculated and cached if it does not already exist.
        /// </summary>
        /// <param name="expectedNode">The expected node.</param>
        /// <param name="actualNode">The actual node.</param>
        /// <returns>The score assigned to node children similarity for the specified nodes.</returns>
        private float GetChildrenScore(Node expectedNode, Node actualNode)
        {
            return ChildrenScores[expectedNode.Index][actualNode.Index] ??= CompareChildren(expectedNode, actualNode);
        }

        /// <summary>
        /// Gets the score assigned to node similarity for the specified nodes.  The score is calculated and cached if it does not already exist.
        /// </summary>
        /// <param name="expectedNode">The expected node.</param>
        /// <param name="actualNode">The actual node.</param>
        /// <returns>The score assigned to node similarity for the specified nodes.</returns>
        private float GetNodeScore(Node expectedNode, Node actualNode)
        {
            return NodeScores[expectedNode.Index][actualNode.Index] ??= expectedNode.CompareTo(actualNode);
        }
    }
}