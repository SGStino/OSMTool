using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class RoadNavigationTest
    {

        [TestMethod]
        public void TestViablePath()
        {
            var roads = new TestRoads();


            Assert.AreEqual(1, roads.seg_0_0_10_0.AIRoutes.Length);
            Assert.AreEqual(1, roads.seg_40_0_50_0.AIRoutes.Length);
            Assert.AreEqual(3, roads.seg_0_0_10_0.AIRoutes[0].Paths.Length);
            Assert.AreEqual(3, roads.seg_40_0_50_0.AIRoutes[0].Paths.Length);

            var routeSolver = new RouteSolver(roads.seg_0_0_10_0.AIRoutes, roads.seg_40_0_50_0.AIRoutes);

            while (!routeSolver.IsComplete)
                routeSolver.Iterate();            
            Assert.IsTrue(routeSolver.IsSuccess);

            var route = routeSolver.Solution;

            var pathSolver = new PathSolver(roads.seg_0_0_10_0.AIRoutes[0].Paths, roads.seg_40_0_50_0.AIRoutes[0].Paths, route);

            while (!pathSolver.IsComplete)
                pathSolver.Iterate();
            Assert.IsTrue(pathSolver.IsSuccess);

        }
    }

    public class TestRoads
    {
        public static LaneDescription highway = new LaneDescription
        {
            LaneType = LaneType.Highway,
            MaxSpeed = 130,
            Reverse = false,
            Turn = Turn.None,
            VehicleTypes = VehicleTypes.Vehicle,
            Width = 4
        };

        public static SegmentDescription highway3Lanes = new SegmentDescription
        {
            Lanes = new[]
            {
                highway,
                highway,
                highway
            }
        };
        public static LaneDescription dirtTrack = new LaneDescription
        {
            LaneType = LaneType.DirtTrack,
            MaxSpeed = 20,
            Reverse = false,
            Turn = Turn.None,
            VehicleTypes = VehicleTypes.Vehicle,
            Width = 2
        };
        public static SegmentDescription singleDirtTrack = new SegmentDescription
        {
            Lanes = new[]
            {
                dirtTrack
            }
        };
        public readonly AIRoadManager aiRoadManager;
        public readonly AINode n_0_0;
        public readonly AINode n_10_0;
        public readonly AINode n_20_0;
        public readonly AINode n_20_10;
        public readonly AINode n_30_0;
        public readonly AINode n_30_10;
        public readonly AINode n_40_0;
        public readonly AINode n_50_0;
        public readonly AISegment seg_0_0_10_0;
        public readonly AISegment seg_10_0_20_10;
        public readonly AISegment seg_10_0_20_0;
        public readonly AISegment seg_20_10_30_10;
        public readonly AISegment seg_20_0_30_0;
        public readonly AISegment seg_30_10_40_0;
        public readonly AISegment seg_30_0_40_0;
        public readonly AISegment seg_40_0_50_0;

        public TestRoads()
        {
            this.aiRoadManager = new AIRoadManager();

            this.n_0_0 = aiRoadManager.CreateNodeAt(0, 0);
            this.n_10_0 = aiRoadManager.CreateNodeAt(10, 0);
            this.n_20_0 = aiRoadManager.CreateNodeAt(20, 0);
            this.n_20_10 = aiRoadManager.CreateNodeAt(20, 10);
            this.n_30_0 = aiRoadManager.CreateNodeAt(30, 0);
            this.n_30_10 = aiRoadManager.CreateNodeAt(30, 10);
            this.n_40_0 = aiRoadManager.CreateNodeAt(40, 0);
            this.n_50_0 = aiRoadManager.CreateNodeAt(50, 0);

            this.seg_0_0_10_0 = aiRoadManager.CreateSegment(n_0_0, n_10_0, highway3Lanes);
            this.seg_10_0_20_10 = aiRoadManager.CreateSegment(n_10_0, n_20_10, highway3Lanes);
            this.seg_10_0_20_0 = aiRoadManager.CreateSegment(n_10_0, n_20_0, singleDirtTrack);

            this.seg_20_10_30_10 = aiRoadManager.CreateSegment(n_20_10, n_30_10, highway3Lanes);
            this.seg_20_0_30_0 = aiRoadManager.CreateSegment(n_10_0, n_20_0, singleDirtTrack);
            this.seg_30_10_40_0 = aiRoadManager.CreateSegment(n_30_10, n_40_0, highway3Lanes);
            this.seg_30_0_40_0 = aiRoadManager.CreateSegment(n_10_0, n_20_0, singleDirtTrack);
            this.seg_40_0_50_0 = aiRoadManager.CreateSegment(n_30_10, n_40_0, highway3Lanes);
        }
    }

}
