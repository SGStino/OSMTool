using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class AIPathExtensionsTest
    {
        [TestMethod]
        public void TestForward()
        {
            var loft = new Lofts.LinearPath(Vector3.zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            {
                PathOffsetStart = 1,
                PathOffsetEnd = 2,
                SideOffsetStart = 3,
                SideOffsetEnd = 4
            };

            Assert.AreEqual(aiPath.PathOffsetStart, aiPath.GetStartPathOffset());
            Assert.AreEqual(10 - aiPath.PathOffsetEnd, aiPath.GetEndPathOffset());
            Assert.AreEqual(aiPath.SideOffsetStart, aiPath.GetStartSideOffset());
            Assert.AreEqual(aiPath.SideOffsetEnd, aiPath.GetEndSideOffset());

            var startTransform = aiPath.GetTransform(0);
            Assert.AreEqual(new Vector3(3, 0, 1), startTransform.MultiplyPoint3x4(Vector3.zero));

            var endTransform = aiPath.GetTransform(aiPath.GetLength());
            Assert.AreEqual(new Vector3(4, 0, 8), endTransform.MultiplyPoint3x4(Vector3.zero));

             startTransform = aiPath.GetStartTransform();
            Assert.AreEqual(new Vector3(3,0,1), startTransform.MultiplyPoint3x4(Vector3.zero));

             endTransform = aiPath.GetEndTransform();
            Assert.AreEqual(new Vector3(4,0,8), endTransform.MultiplyPoint3x4(Vector3.zero));


            Assert.AreEqual(7, aiPath.GetLength());

            var halfLength = 3.5f;

            var halfTransform =  aiPath.GetTransform(halfLength);

            Assert.AreEqual(new Vector3(3.5f, 0, 4.5f), halfTransform.MultiplyPoint3x4(Vector3.zero));

            Assert.AreEqual(new Vector3(0, 0, 1), halfTransform.MultiplyVector(Vector3.forward));
        }
        [TestMethod]
        public void TestBackward()
        {
            var loft = new Lofts.LinearPath(Vector3.zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            {
                PathOffsetStart = 1,
                PathOffsetEnd = 2,
                SideOffsetStart = 3,
                SideOffsetEnd = 4,
                Reverse = true
            };

            Assert.AreEqual(10 - aiPath.PathOffsetEnd, aiPath.GetStartPathOffset());
            Assert.AreEqual(aiPath.PathOffsetStart, aiPath.GetEndPathOffset());
            Assert.AreEqual(aiPath.SideOffsetEnd, aiPath.GetStartSideOffset());
            Assert.AreEqual(aiPath.SideOffsetStart, aiPath.GetEndSideOffset());

            var startTransform = aiPath.GetTransform(0);
            Assert.AreEqual(new Vector3(4, 0, 8), startTransform.MultiplyPoint3x4(Vector3.zero));

            var endTransform = aiPath.GetTransform(aiPath.GetLength());
            Assert.AreEqual(new Vector3(3, 0, 1), endTransform.MultiplyPoint3x4(Vector3.zero));

            startTransform = aiPath.GetStartTransform();
            Assert.AreEqual(new Vector3(4, 0, 8), startTransform.MultiplyPoint3x4(Vector3.zero));

            endTransform = aiPath.GetEndTransform();
            Assert.AreEqual(new Vector3(3, 0, 1), endTransform.MultiplyPoint3x4(Vector3.zero));


            Assert.AreEqual(7, aiPath.GetLength());

            var halfLength = 3.5f;

            var halfTransform = aiPath.GetTransform(halfLength);

            Assert.AreEqual(new Vector3(3.5f, 0, 4.5f), halfTransform.MultiplyPoint3x4(Vector3.zero));

            Assert.AreEqual(new Vector3(0, 0, -1), halfTransform.MultiplyVector(Vector3.forward));
        }
    }

    internal class DummyAIPath : IAIPath
    {
        private LinearPath loft;

        public DummyAIPath(LinearPath loft)
        {
            this.loft = loft;
        }

        public ILoftPath LoftPath => loft;

        public float SideOffsetStart { get; set; }

        public float SideOffsetEnd { get; set; }

        public float PathOffsetStart { get; set; }

        public float PathOffsetEnd { get; set; }

        public IAIPath LeftParralel => null;

        public IAIPath RightParralel => null;

        public bool Reverse { get; set; }

        public float MaxSpeed => 1;

        public float AverageSpeed => 1;

        public IEnumerable<IAIPath> NextPaths => Enumerable.Empty<IAIPath>();

        public LaneType LaneType => LaneType.Road;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths;
    }
}
