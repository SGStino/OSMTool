using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class NodeSegmentNetworkTest
    {
        [TestMethod]
        public void TestConnections()
        {
            var nodeDescription = new NodeDescription
            {
                Factory = NodeAIPathsFactory.Default
            };
            var segmentDescription = new SegmentDescription
            {
                Lanes = new[]
                {
                    new LaneDescription
                    {
                         LaneType = LaneType.Road,
                         Reverse = false,
                         VehicleTypes = VehicleTypes.Vehicle,
                         Width =3
                    },
                    new LaneDescription
                    {
                         LaneType = LaneType.Road,
                         Reverse = true,
                         VehicleTypes = VehicleTypes.Vehicle,
                         Width =3
                    }
                },
                SegmentFactory = SegmentAIPathsFactory.Default
            };
            var nodeA = Node.CreateAt(-15, 0, nodeDescription);
            var nodeB = Node.CreateAt(15, 0, nodeDescription);
            var nodeC = Node.CreateAt(0, 10, nodeDescription);

            var segmentAB = Segment.Create(nodeA, nodeB, segmentDescription);
            var segmentBC = Segment.Create(nodeB, nodeC, segmentDescription);
            var segmentCA = Segment.Create(nodeC, nodeA, segmentDescription);

            Assert.AreEqual(2, segmentAB.AIRoutes.Count);
            Assert.AreEqual(2, segmentBC.AIRoutes.Count);
            Assert.AreEqual(2, segmentCA.AIRoutes.Count);

            Assert.IsNotNull(segmentAB.LoftPath.Value);
            Assert.IsNotNull(segmentBC.LoftPath.Value);
            Assert.IsNotNull(segmentCA.LoftPath.Value);


            Assert.AreEqual(2, nodeA.Connections.Value.Count);
            Assert.AreEqual(2, nodeB.Connections.Value.Count);
            Assert.AreEqual(2, nodeB.Connections.Value.Count);



            Assert.AreEqual(1, nodeA.Connections.Value[0].OutgoingAIRoutes.Value.Count);
            Assert.AreEqual(1, nodeA.Connections.Value[1].OutgoingAIRoutes.Value.Count);
            Assert.AreEqual(1, nodeB.Connections.Value[0].OutgoingAIRoutes.Value.Count);
            Assert.AreEqual(1, nodeB.Connections.Value[1].OutgoingAIRoutes.Value.Count);
            Assert.AreEqual(1, nodeC.Connections.Value[0].OutgoingAIRoutes.Value.Count);
            Assert.AreEqual(1, nodeC.Connections.Value[1].OutgoingAIRoutes.Value.Count);
        }
    }
}
