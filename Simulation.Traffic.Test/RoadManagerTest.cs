using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Simulation.Traffic;
using System.Linq;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class RoadManagerTests
    {
        private static SegmentDescription dummyRoad = new SegmentDescription
        {
            Lanes = new[]
            {
                new LaneDescription
                {
                }
            }
        };

        [TestMethod]
        public void TestRemoveNode()
        {
            var roads = new RoadManager();

            var n1 = roads.CreateNodeAt(1, 0);

            var n2 = roads.CreateNodeAt(0, 1);

            var n3 = roads.CreateNodeAt(0, -1);


            var s12 = roads.CreateSegment(n1, n2, dummyRoad);
            var s23 = roads.CreateSegment(n2, n3, dummyRoad);
            var s31 = roads.CreateSegment(n3, n1, dummyRoad);

            Assert.AreEqual(2, n1.Segments.Count());
            Assert.AreEqual(2, n2.Segments.Count());
            Assert.AreEqual(2, n3.Segments.Count());

            Assert.IsTrue(roads.Remove(n3));


            Assert.AreEqual(1, n1.Segments.Count());
            Assert.AreEqual(1, n2.Segments.Count());

            CollectionAssert.AreEquivalent(new[] { s12 }, roads.Segments.ToArray());
            CollectionAssert.AreEquivalent(new[] { n1, n2 }, roads.Nodes.ToArray());
        }
        [TestMethod]
        public void TestRemoveSegment()
        {
            var roads = new RoadManager();

            var n1 = roads.CreateNodeAt(1, 0);

            var n2 = roads.CreateNodeAt(0, 1);

            var n3 = roads.CreateNodeAt(0, -1);


            var s12 = roads.CreateSegment(n1, n2, dummyRoad);
            var s23 = roads.CreateSegment(n2, n3, dummyRoad);
            var s31 = roads.CreateSegment(n3, n1, dummyRoad);

            Assert.AreEqual(2, n1.Segments.Count());
            Assert.AreEqual(2, n2.Segments.Count());
            Assert.AreEqual(2, n3.Segments.Count());

            Assert.IsTrue(roads.Remove(s31));


            CollectionAssert.AreEquivalent(new[] { s12, s23 }, roads.Segments.ToArray());
            CollectionAssert.AreEquivalent(new[] { n1, n2, n3 }, roads.Nodes.ToArray());

            Assert.AreEqual(1, n1.Segments.Count());
            Assert.AreEqual(2, n2.Segments.Count());
            Assert.AreEqual(1, n3.Segments.Count());
        }


        [TestMethod]
        public void TestMergeSegments()
        {
            var roads = new RoadManager();

            var n1 = roads.CreateNodeAt(0, 0);

            var n2 = roads.CreateNodeAt(0, 1);

            var n3 = roads.CreateNodeAt(0, -1);


            var s12 = roads.CreateSegment(n1, n2, dummyRoad);
            var s23 = roads.CreateSegment(n2, n3, dummyRoad);

            var s13 = roads.MergeSegments(n2);

            Assert.AreEqual(n1, s13.Start.Node);
            Assert.AreEqual(n3, s13.End.Node);

            CollectionAssert.AreEquivalent(new[] { s13 }, roads.Segments.ToArray());
        }


        [TestMethod]
        public void TestMergeNodes()
        {
            var roads = new RoadManager();

            var n1 = roads.CreateNodeAt(0, 0);

            var n2 = roads.CreateNodeAt(0, 1);

            var n3 = roads.CreateNodeAt(0, -1);


            var s12 = roads.CreateSegment(n1, n2, dummyRoad);
            var s23 = roads.CreateSegment(n2, n3, dummyRoad);

            var n23 = roads.MergeNodes(n2, n3);


            Assert.AreEqual(1, roads.Segments.Count());
            CollectionAssert.AreEquivalent(new[] { n1, n23 }, roads.Nodes.ToArray());
        }
    }
}
