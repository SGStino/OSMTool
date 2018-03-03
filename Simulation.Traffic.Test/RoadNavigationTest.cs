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


            Assert.AreEqual(1, roads.seg_0_0_20_0.AIRoutes.Length);
            Assert.AreEqual(1, roads.seg_80_0_100_0.AIRoutes.Length);
            Assert.AreEqual(3, roads.seg_0_0_20_0.AIRoutes[0].Paths.Length);
            Assert.AreEqual(3, roads.seg_80_0_100_0.AIRoutes[0].Paths.Length);

            var routeSolver = new RouteSolver(roads.seg_0_0_20_0.AIRoutes, roads.seg_80_0_100_0.AIRoutes);

            while (!routeSolver.IsComplete)
                routeSolver.Iterate();            
            Assert.IsTrue(routeSolver.IsSuccess);

            var route = routeSolver.Solution;

            var pathSolver = new PathSolver(roads.seg_0_0_20_0.AIRoutes[0].Paths, roads.seg_80_0_100_0.AIRoutes[0].Paths, route);

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
        public readonly AINode n_20_0;
        public readonly AINode n_40_0;
        public readonly AINode n_40_20;
        public readonly AINode n_60_0;
        public readonly AINode n_60_20;
        public readonly AINode n_80_0;
        public readonly AINode n_100_0;
        public readonly AISegment seg_0_0_20_0;
        public readonly AISegment seg_20_0_40_20;
        public readonly AISegment seg_20_0_40_0;
        public readonly AISegment seg_40_20_60_20;
        public readonly AISegment seg_40_0_60_0;
        public readonly AISegment seg_60_20_80_0;
        public readonly AISegment seg_60_0_80_0;
        public readonly AISegment seg_80_0_100_0;

        public TestRoads()
        {
            this.aiRoadManager = new AIRoadManager();

            this.n_0_0 = aiRoadManager.CreateNodeAt(0, 0);
            this.n_20_0 = aiRoadManager.CreateNodeAt(20, 0);
            this.n_40_0 = aiRoadManager.CreateNodeAt(40, 0);
            this.n_40_20 = aiRoadManager.CreateNodeAt(40, 20);
            this.n_60_0 = aiRoadManager.CreateNodeAt(60, 0);
            this.n_60_20 = aiRoadManager.CreateNodeAt(60, 20);
            this.n_80_0 = aiRoadManager.CreateNodeAt(80, 0);
            this.n_100_0 = aiRoadManager.CreateNodeAt(100, 0);

            this.seg_0_0_20_0 = aiRoadManager.CreateSegment(n_0_0, n_20_0, highway3Lanes);
            this.seg_20_0_40_20 = aiRoadManager.CreateSegment(n_20_0, n_40_20, highway3Lanes);
            this.seg_20_0_40_0 = aiRoadManager.CreateSegment(n_20_0, n_40_0, singleDirtTrack);

            this.seg_40_20_60_20 = aiRoadManager.CreateSegment(n_40_20, n_60_20, highway3Lanes);
            this.seg_40_0_60_0 = aiRoadManager.CreateSegment(n_20_0, n_40_0, singleDirtTrack);
            this.seg_60_20_80_0 = aiRoadManager.CreateSegment(n_60_20, n_80_0, highway3Lanes);
            this.seg_60_0_80_0 = aiRoadManager.CreateSegment(n_20_0, n_40_0, singleDirtTrack);
            this.seg_80_0_100_0 = aiRoadManager.CreateSegment(n_60_20, n_80_0, highway3Lanes); 
        }
    }

}
