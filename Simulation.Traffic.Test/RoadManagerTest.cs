using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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
        public static void TestMergeSegments()
        {
            var roads = new RoadManager();
                        
            var n1 = roads.CreateNodeAt(0, 0);

            var n2 = roads.CreateNodeAt(0, 1);

            var n3 = roads.CreateNodeAt(0, -1);


            var s12 = roads.CreateSegment(n1, n2, dummyRoad);
            var s23 = roads.CreateSegment(n1, n2, dummyRoad);

            roads.MergeSegments(n2);
        }
    }
}
