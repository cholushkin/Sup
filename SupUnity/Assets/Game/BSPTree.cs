using System.Collections.Generic;
using GameLib.Random;
using UnityEngine;
using UnityEngine.Assertions;

namespace BSPTreeRect
{
    public class BspTree
    {
        public class Node
        {
            public Node Parent;
            public (Node, Node) Children;
            public Rect Rect;

            public Rect GetAbsoluteRect()
            {
                if (Parent == null)
                    return Rect;
                var parentPos = Parent.GetAbsoluteRect().position;
                return new Rect(parentPos + Rect.position, Rect.size);
            }
        }
        public Node Root;
    }

    public static class BspTreeHelper
    {
        public class BspTreeGeneratorParams
        {
            public Range MinNodeWidth = new(3, 6);
            public Range MinNodeHeight = new(3, 6);
        }

        private static readonly BspTreeGeneratorParams DefaultGeneratorParams = new();

        public static BspTree GenerateBspTree(BspTree.Node root, BspTreeGeneratorParams genParams = default, int seed = -1)
        {
            genParams = genParams ?? DefaultGeneratorParams;

            var tree = new BspTree { Root = root };
            var rnd = RandomHelper.CreateRandomNumberGenerator(seed);

            Queue<BspTree.Node> toSplit = new Queue<BspTree.Node>();
            toSplit.Enqueue(root);
            while (toSplit.Count > 0)
            {
                var curNode = toSplit.Dequeue();
                var minWidth = Mathf.Round(rnd.FromRange(genParams.MinNodeWidth));
                var minHeight = Mathf.Round(rnd.FromRange(genParams.MinNodeHeight));

                // can split?
                const float epsOffset = 0.1f;
                var isEnoughParentWidth = minWidth + minWidth + minWidth * epsOffset < curNode.Rect.width; // + 10% of min aspect
                var isEnoughParentHeight = minHeight + minHeight + minHeight * epsOffset < curNode.Rect.height;

                if (isEnoughParentWidth && isEnoughParentHeight)
                {
                    bool splitVert = curNode.Rect.width > curNode.Rect.height;
                    float splitLine = Mathf.Round(splitVert ? rnd.Range(minWidth, curNode.Rect.width - minWidth)
                        : rnd.Range(minHeight, curNode.Rect.height - minHeight));

                    SplitNode(curNode, splitLine, splitVert);

                    toSplit.Enqueue(curNode.Children.Item1);
                    toSplit.Enqueue(curNode.Children.Item2);
                }
            }
            return tree;
        }

        public static (BspTree.Node, BspTree.Node) SplitNode(BspTree.Node node, float splitLine, bool isSplitLineVert)
        {
            Assert.IsNotNull(node);
            Assert.IsTrue(splitLine != 0);

            var child1 = new BspTree.Node();
            var child2 = new BspTree.Node();

            if (isSplitLineVert)
            {
                child1.Rect = new Rect(Vector2.zero, new Vector2(splitLine, node.Rect.height));
                child2.Rect = new Rect(new Vector2(splitLine, 0), new Vector2(node.Rect.width - splitLine, node.Rect.height));
            }
            else
            {
                child1.Rect = new Rect(Vector2.zero, new Vector2(node.Rect.width, splitLine));
                child2.Rect = new Rect(new Vector2(0, splitLine), new Vector2(node.Rect.width, node.Rect.height - splitLine));
            }

            child1.Parent = node;
            child2.Parent = node;
            node.Children = (child1, child2);
            return node.Children;
        }

        public static List<Rect> GetTopNodes(this BspTree bspTree)
        {
            var topNodes = new List<Rect>();
            WalkGetTopNodes(bspTree.Root, ref topNodes);
            return topNodes;
        }

        private static void WalkGetTopNodes(BspTree.Node node, ref List<Rect> topNodes)
        {
            var topMost = node.Children.Item1 == null;
            Assert.IsTrue(topMost == (node.Children.Item2 == null));

            if (!topMost)
            {
                WalkGetTopNodes(node.Children.Item1, ref topNodes);
                WalkGetTopNodes(node.Children.Item2, ref topNodes);
            }
            else // this node is upper level
            {
                topNodes.Add(node.GetAbsoluteRect());
            }
        }
    }
}
