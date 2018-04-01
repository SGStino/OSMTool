using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data.Trees;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Data.Test
{
    [TestClass]
    public class QuadTreeTests
    {
        private class DummyItem : IBoundsObject2D
        {
            private Rect bounds;

            public Rect Bounds
            {
                get => bounds; set
                {
                    if (bounds != value)
                    {
                        var old = bounds;
                        bounds = value;
                    }
                }
            }

            public event Action<BoundsChangedEvent> BoundsChanged;
        }

        [TestMethod]
        public void TestBigAddWithoutGrow()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy = new DummyItem { Bounds = Rect.MinMaxRect(0.25f, 0.25f, 0.75f, 0.75f) };

            var node = tree.Add(dummy);


            Assert.AreEqual(root, tree.CurrentRoot);
            Assert.AreEqual(root, node);
        }


        [TestMethod]
        public void TestRemoveFromMultipleNodes()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy1 = new DummyItem { Bounds = Rect.MinMaxRect(1.25f, 1.24f, 1.75f, 1.76f) };

            var dummy2 = new DummyItem { Bounds = Rect.MinMaxRect(-1.24f, 1.25f, -1.76f, 1.75f) };

            var dummy3 = new DummyItem { Bounds = Rect.MinMaxRect(1.25f, -1.24f, 1.75f, -1.76f) };

            var dummy4 = new DummyItem { Bounds = Rect.MinMaxRect(-1.24f, -1.25f, -1.76f, -1.75f) };

            var node1 = tree.Add(dummy1);
            var node2 = tree.Add(dummy2);
            var node3 = tree.Add(dummy3);
            var node4 = tree.Add(dummy4);


            Assert.AreNotEqual(root, tree.CurrentRoot);
            Assert.AreNotEqual(root, node1);
            Assert.AreNotEqual(root, node2);
            Assert.AreNotEqual(root, node3);
            Assert.AreNotEqual(root, node4);



            Assert.IsTrue(tree.Remove(dummy1));

            Assert.IsFalse(node1.Items.Contains(dummy1), "node contains dummy 1");
            Assert.IsTrue(node2.Items.Contains(dummy2), "node contains dummy 2");
            Assert.IsTrue(node3.Items.Contains(dummy3), "node contains dummy 3");
            Assert.IsTrue(node4.Items.Contains(dummy4), "node contains dummy 4");


            Assert.IsTrue(tree.Remove(dummy2));

            Assert.IsFalse(node1.Items.Contains(dummy1), "node contains dummy 1");
            Assert.IsFalse(node2.Items.Contains(dummy2), "node contains dummy 2");
            Assert.IsTrue(node3.Items.Contains(dummy3), "node contains dummy 3");
            Assert.IsTrue(node4.Items.Contains(dummy4), "node contains dummy 4");


            Assert.IsTrue(tree.Remove(dummy3));

            Assert.IsFalse(node1.Items.Contains(dummy1), "node contains dummy 1");
            Assert.IsFalse(node2.Items.Contains(dummy2), "node contains dummy 2");
            Assert.IsFalse(node3.Items.Contains(dummy3), "node contains dummy 3");
            Assert.IsTrue(node4.Items.Contains(dummy4), "node contains dummy 4");

            Assert.IsTrue(tree.Remove(dummy4));

            Assert.IsFalse(node1.Items.Contains(dummy1), "node contains dummy 1");
            Assert.IsFalse(node2.Items.Contains(dummy2), "node contains dummy 2");
            Assert.IsFalse(node3.Items.Contains(dummy3), "node contains dummy 3");
            Assert.IsFalse(node4.Items.Contains(dummy4), "node contains dummy 4");

        }


        [TestMethod]
        public void TestRemoveFromSingleNode()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy1 = new DummyItem { Bounds = Rect.MinMaxRect(0.25f, 0.24f, 0.75f, 0.76f) };

            var dummy2 = new DummyItem { Bounds = Rect.MinMaxRect(0.24f, 0.25f, 0.76f, 0.75f) };

            var node1 = tree.Add(dummy1);
            var node2 = tree.Add(dummy2);


            Assert.AreEqual(root, tree.CurrentRoot);
            Assert.AreEqual(root, node1);
            Assert.AreEqual(root, node2);



            Assert.IsTrue(tree.Remove(dummy1));

            Assert.IsFalse(node1.Items.Contains(dummy1), "node contains dummy 1");
            Assert.IsTrue(node2.Items.Contains(dummy2), "node contains dummy 2");
        }


        [TestMethod]
        public void TestTinyAddWithoutGrow()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy = new DummyItem { Bounds = Rect.MinMaxRect(0.25f, 0.25f, 0.26f, 0.26f) };

            var node = tree.Add(dummy);


            Assert.AreEqual(root, tree.CurrentRoot);
            Assert.AreNotEqual(root, node);
        }

        [TestMethod]
        public void TestAddWithGrow()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy = new DummyItem { Bounds = Rect.MinMaxRect(-1.25f, -1.25f, -0.75f, -0.75f) };

            var node = tree.Add(dummy);


            Assert.AreNotEqual(root, tree.CurrentRoot);
        }

        [TestMethod]
        public void TestAddFar()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 10, 10));

            var root = tree.CurrentRoot;

            var dummy = new DummyItem { Bounds = new Rect(-99.5f, -99.5f, 1, 1) };

            var node = tree.Add(dummy);

            validateNodeContainment(dummy, node);


            Assert.AreNotEqual(root, tree.CurrentRoot);
            validateNode(tree.CurrentRoot, "");
        }

        private void validateNodeContainment(DummyItem dummy, QuadTreeNode<DummyItem> node)
        {
            Assert.IsTrue(node.Bounds.Contains(dummy.Bounds.center));

            if (node.Parent != null)
                validateNodeContainment(dummy, node.Parent);
        }

        [TestMethod]
        public void TestQueryBounds()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy1 = new DummyItem { Bounds = Rect.MinMaxRect(-1.25f, -1.25f, -0.75f, -0.75f) };

            var dummy2 = new DummyItem { Bounds = Rect.MinMaxRect(0.25f, 0.25f, 0.75f, 0.75f) };

            var node1 = tree.Add(dummy1);
            var node2 = tree.Add(dummy2);


            Assert.AreNotEqual(root, tree.CurrentRoot);

            var items1 = tree.Query(dummy1.Bounds);
            var items2 = tree.Query(dummy2.Bounds);

            Assert.AreEqual(dummy1, items1.Single());
            Assert.AreEqual(dummy2, items2.Single());
        }

        [TestMethod]
        public void TestQueryRadius()
        {
            var tree = new QuadTree<DummyItem>(new Rect(0, 0, 1, 1));

            var root = tree.CurrentRoot;

            var dummy1 = new DummyItem { Bounds = Rect.MinMaxRect(-1.25f, -1.25f, -0.75f, -0.75f) };

            var dummy2 = new DummyItem { Bounds = Rect.MinMaxRect(0.25f, 0.25f, 0.75f, 0.75f) };

            var node1 = tree.Add(dummy1);
            var node2 = tree.Add(dummy2);


            Assert.AreNotEqual(root, tree.CurrentRoot);

            var items1 = tree.Query(dummy1.Bounds.center, 1);
            var items2 = tree.Query(dummy2.Bounds.center, 1);

            Assert.AreEqual(dummy1, items1.Single());
            Assert.AreEqual(dummy2, items2.Single());
        }


        [TestMethod]
        public void TestBulk()
        {
            var rnd = new System.Random(15);

            var rectangles = new DummyItem[100000];
            var tree = new QuadTree<DummyItem>(new Rect(-1000, -1000, 2000, 2000));


            Parallel.For(0, rectangles.Length, i =>
            //for (int i = 0; i < rectangles.Length; i++)
            {
                var x = (float)(rnd.NextDouble() * 2 - 1) * 64000;
                var y = (float)(rnd.NextDouble() * 2 - 1) * 64000;
                var w = (float)rnd.NextDouble() * 2 + 1;
                var h = (float)rnd.NextDouble() * 2 + 1;
                x -= w / 2;
                y -= h / 2;

                var dummy = new DummyItem
                {
                    Bounds = new Rect(x, y, w, h)
                };
                rectangles[i] = dummy;
                var n = tree.Add(dummy);
                validateNodeContainment(dummy, n);
            });

            var nulls = tree.Any(n => n == null);
            Assert.IsFalse(nulls);


            int removeCount = rectangles.Length / 2;
            var removes = rectangles.Take(removeCount).ToArray();


            removes.AsParallel().ForAll(n => tree.Remove(n));

            Assert.AreEqual(rectangles.Length - removeCount, tree.Count);

            nulls = tree.Any(n => n == null);
            Assert.IsFalse(nulls);

            validateNode(tree.CurrentRoot, "");
        }

        private void validateNode(QuadTreeNode<DummyItem> currentRoot, string path)
        {
            var quads = QuadTreeUtils.DevideQuads(currentRoot.Bounds);

            var children = currentRoot.Children;
            if (children != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    Assert.AreEqual(children[i].Bounds, quads[i], "validating " + path + " node " + i);
                    validateNode(children[i], path + "/" + i);
                }
            }
        }
    }
}
