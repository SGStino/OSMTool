using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data.Trees;
using UnityEngine;
using System.Reactive.Linq;
using System;
using System.Reactive.Subjects;

namespace Simulation.Data.Test
{
    [TestClass]
    public class QuadTreeObservablesTest
    {
        [TestMethod]
        public void TestEventLevels()
        {
            var topLeft = new DummyItem(new Rect(-0.75f, -0.75f, 0.125f, 0.125f));


            var quadTree = new QuadTree<DummyItem>(new Rect(-1, -1, 2, 2));


            int eventsInRoot = 0;
            int eventsInLeaf = 0;
            quadTree.CurrentRoot.Subscribe(evt => eventsInRoot++);


            Assert.AreEqual(0, eventsInRoot);

            var node = quadTree.Add(topLeft);

            node.Subscribe(evt => eventsInLeaf++);
            Assert.AreEqual(1, eventsInRoot);
            Assert.AreEqual(0, eventsInLeaf);

            quadTree.Remove(topLeft);

            Assert.AreEqual(2, eventsInRoot);
            Assert.AreEqual(1, eventsInLeaf);

        }


        [TestMethod]
        public void TestObservableQueries()
        {
            var topLeft = new DummyItem(new Rect(-0.75f, -0.75f, 0.25f, 0.25f));

            var quadTree = new QuadTree<DummyItem>(new Rect(-1, -1, 2, 2));

            var topLeftQuery = quadTree.Observe(new Rect(-1, -1, 1, 1));
            var topRightQuery = quadTree.Observe(new Rect(0, -1, 1, 1));

            var overlapQuery = quadTree.Observe(new Rect(-0.6f, -0.6f, 1, 1));
            int topLeftQueryCount = 0, topRightQueryCount = 0, overlapQueryCount = 0;
            topLeftQuery.Subscribe(t => topLeftQueryCount++);
            topRightQuery.Subscribe(t => topRightQueryCount++);
            overlapQuery.Subscribe(t => overlapQueryCount++);



            quadTree.Add(topLeft);

            Assert.AreEqual(1, topLeftQueryCount);
            Assert.AreEqual(0, topRightQueryCount);
            Assert.AreEqual(1, overlapQueryCount);

            quadTree.Remove(topLeft);

            Assert.AreEqual(2, topLeftQueryCount);
            Assert.AreEqual(0, topRightQueryCount);
            Assert.AreEqual(2, overlapQueryCount);

        }
        [TestMethod]
        public void TestObservableCenterQueries()
        {
            var topLeft = new DummyItem(new Rect(-0.75f, -0.75f, 0.25f, 0.25f));

            var quadTree = new QuadTree<DummyItem>(new Rect(-1, -1, 2, 2));

            var topLeftQuery = quadTree.ObserveCenter(new Rect(-1, -1, 1, 1));
            var topRightQuery = quadTree.ObserveCenter(new Rect(0, -1, 1, 1));

            var overlapQuery = quadTree.ObserveCenter(new Rect(-0.6f, -0.6f, 1, 1));
            int topLeftQueryCount = 0, topRightQueryCount = 0, overlapQueryCount = 0;
            topLeftQuery.Subscribe(t => topLeftQueryCount++);
            topRightQuery.Subscribe(t => topRightQueryCount++);
            overlapQuery.Subscribe(t => overlapQueryCount++);



            quadTree.Add(topLeft);

            Assert.AreEqual(1, topLeftQueryCount);
            Assert.AreEqual(0, topRightQueryCount);
            Assert.AreEqual(0, overlapQueryCount);

            quadTree.Remove(topLeft);

            Assert.AreEqual(2, topLeftQueryCount);
            Assert.AreEqual(0, topRightQueryCount);
            Assert.AreEqual(0, overlapQueryCount);
        }


        [TestMethod]
        public void TestObservableQueriesMoveOutside()
        {
            var topLeft = new DummyItem(new Rect(-0.75f, -0.75f, 0.25f, 0.25f));

            var quadTree = new QuadTree<DummyItem>(new Rect(-1, -1, 2, 2));

            var topLeftQuery = quadTree.Observe(new Rect(-1, -1, 1, 1));
            var topRightQuery = quadTree.Observe(new Rect(0, -1, 1, 1));

            int topLeftQueryCount = 0, topRightQueryCount = 0;
            topLeftQuery.Subscribe(t => topLeftQueryCount++);
            topRightQuery.Subscribe(t => topRightQueryCount++);



            quadTree.Add(topLeft);

            Assert.AreEqual(1, topLeftQueryCount);
            Assert.AreEqual(0, topRightQueryCount);


            topLeft.Move(new Rect(0.5f, -0.75f, 0.25f, 0.25f));
            Assert.AreEqual(2, topLeftQueryCount);
            Assert.AreEqual(1, topRightQueryCount);

            quadTree.Remove(topLeft);

            Assert.AreEqual(2, topLeftQueryCount);
            Assert.AreEqual(2, topRightQueryCount);

        }
    }

    internal class DummyItem : IBoundsObject2D
    {
        public DummyItem(Rect bounds)
        {
            Bounds = bounds;
        }

        public Rect Bounds { get; private set; }

        public event Action<BoundsChangedEvent> BoundsChanged;

        public void Move(Rect newBounds)
        {
            Bounds = newBounds;
            BoundsChanged?.Invoke(new BoundsChangedEvent(newBounds));
        }
    }
}
