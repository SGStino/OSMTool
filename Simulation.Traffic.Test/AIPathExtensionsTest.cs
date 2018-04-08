using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data;
using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class AIPathExtensionsTest
    {
        [DataTestMethod]
        [DataRow(5, 3)]
        [DataRow(4, 2)]
        [DataRow(3, 1)]
        [DataRow(2, 0)]
        public void TestGetDistanceFromLoftForward(float distance, float expected)
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            {
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 2,
                pathOffsetEnd: 5,
                sideOffsetStart: 0,
                sideOffsetEnd: 0);

            var result = aiPath.GetDistanceFromLoftPath(distance);


            Assert.AreEqual(expected, result);

        }
        [DataTestMethod]
        [DataRow(5, 0)]
        [DataRow(4, 1)]
        [DataRow(3, 2)]
        [DataRow(2, 3)]
        public void TestGetDistanceFromLoftBackward(float distance, float expected)
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));


            var aiPath = new DummyAIPath(loft)
            {
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 2,
                pathOffsetEnd: 5,
                sideOffsetStart: 0,
                sideOffsetEnd: 0);

            var result = aiPath.GetDistanceFromLoftPath(distance);


            Assert.AreEqual(expected, result);

        }

        [TestMethod]
        public void TestForward()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);

            Assert.AreEqual(aiPath.Offsets.Value.PathOffsetStart, aiPath.GetStartPathOffset());
            Assert.AreEqual(10 - aiPath.Offsets.Value.PathOffsetEnd, aiPath.GetEndPathOffset());
            Assert.AreEqual(aiPath.Offsets.Value.SideOffsetStart, aiPath.GetStartSideOffset());
            Assert.AreEqual(aiPath.Offsets.Value.SideOffsetEnd, aiPath.GetEndSideOffset());

            var startTransform = aiPath.GetTransform(0);
            Assert.AreEqual(new Vector3(3, 0, 1), startTransform.MultiplyPoint3x4(Vector3.Zero));

            var endTransform = aiPath.GetTransform(aiPath.GetLength());
            Assert.AreEqual(new Vector3(4, 0, 8), endTransform.MultiplyPoint3x4(Vector3.Zero));

            startTransform = aiPath.GetStartTransform();
            Assert.AreEqual(new Vector3(3, 0, 1), startTransform.MultiplyPoint3x4(Vector3.Zero));

            endTransform = aiPath.GetEndTransform();
            Assert.AreEqual(new Vector3(4, 0, 8), endTransform.MultiplyPoint3x4(Vector3.Zero));


            Assert.AreEqual(7, aiPath.GetLength());

            var halfLength = 3.5f;

            var halfTransform = aiPath.GetTransform(halfLength);

            Assert.AreEqual(new Vector3(3.5f, 0, 4.5f), halfTransform.MultiplyPoint3x4(Vector3.Zero));

            Assert.AreEqual(new Vector3(0, 0, 1), halfTransform.MultiplyVector(Directions3.Forward));
        }
        [TestMethod]
        public void TestBackward()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);

            Assert.AreEqual(10 - aiPath.Offsets.Value.PathOffsetEnd, aiPath.GetStartPathOffset());
            Assert.AreEqual(aiPath.Offsets.Value.PathOffsetStart, aiPath.GetEndPathOffset());
            Assert.AreEqual(aiPath.Offsets.Value.SideOffsetEnd, aiPath.GetStartSideOffset());
            Assert.AreEqual(aiPath.Offsets.Value.SideOffsetStart, aiPath.GetEndSideOffset());

            var startTransform = aiPath.GetTransform(0);
            Assert.AreEqual(new Vector3(4, 0, 8), startTransform.MultiplyPoint3x4(Vector3.Zero));

            var endTransform = aiPath.GetTransform(aiPath.GetLength());
            Assert.AreEqual(new Vector3(3, 0, 1), endTransform.MultiplyPoint3x4(Vector3.Zero));

            startTransform = aiPath.GetStartTransform();
            Assert.AreEqual(new Vector3(4, 0, 8), startTransform.MultiplyPoint3x4(Vector3.Zero));

            endTransform = aiPath.GetEndTransform();
            Assert.AreEqual(new Vector3(3, 0, 1), endTransform.MultiplyPoint3x4(Vector3.Zero));


            Assert.AreEqual(7, aiPath.GetLength());

            var halfLength = 3.5f;

            var halfTransform = aiPath.GetTransform(halfLength);

            Assert.AreEqual(new Vector3(3.5f, 0, 4.5f), halfTransform.MultiplyPoint3x4(Vector3.Zero));

            Assert.AreEqual(new Vector3(0, 0, -1), halfTransform.MultiplyVector(Directions3.Forward));
        }
        [TestMethod]
        public void TestStartTransformReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var start = aiPath.GetStartTransform().MultiplyPoint3x4(Vector3.Zero);
            var zero = aiPath.GetTransform(0).MultiplyPoint3x4(Vector3.Zero);
            var expected = new Vector3(4, 0, 8);

            Assert.AreEqual(expected, zero);
            Assert.AreEqual(expected, start);
        }
        [TestMethod]
        public void TestStartTransform()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var start = aiPath.GetStartTransform().MultiplyPoint3x4(Vector3.Zero);
            var zero = aiPath.GetTransform(0).MultiplyPoint3x4(Vector3.Zero);
            var expected = new Vector3(3, 0, 1);

            Assert.AreEqual(expected, zero);
            Assert.AreEqual(expected, start);
        }

        [TestMethod]
        public void TestSnapTo()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 5.5f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(5.5f, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(4.5f, distancePath);
        }
        [TestMethod]
        public void TestSnapToLow()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 0.5f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(0.5f, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(0, distancePath);
        }
        [TestMethod]
        public void TestSnapToMin()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, -1f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(0, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(0, distancePath);
        }
        [TestMethod]
        public void TestSnapToHigh()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 9);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(9, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(7, distancePath);
        }
        [TestMethod]
        public void TestSnapToMax()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 11);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(10, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(7, distancePath);
        }


        [TestMethod]
        public void TestSnapToReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 5.5f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(5.5f, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(2.5f, distancePath);
        }
        [TestMethod]
        public void TestSnapToLowReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 0.5f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(0.5f, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(7, distancePath);
        }
        [TestMethod]
        public void TestSnapToMinReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, -1f);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(0, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(7, distancePath);
        }
        [TestMethod]
        public void TestSnapToHighReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 9);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(9, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(0, distancePath);
        }
        [TestMethod]
        public void TestSnapToMaxReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var pointCenter = new Vector3(0, 0, 11);
            loft.SnapTo(pointCenter, out var pointCenter2, out float distanceLoft);
            Assert.AreEqual(10, distanceLoft);
            aiPath.SnapTo(pointCenter, out var distancePath);
            Assert.AreEqual(0, distancePath);
        }


        [TestMethod]
        public void TestEndTransformReverse()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            { 
                Reverse = true
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart: 1,
                pathOffsetEnd: 2,
                sideOffsetStart: 3,
                sideOffsetEnd: 4);


            var end = aiPath.GetEndTransform().MultiplyPoint3x4(Vector3.Zero);
            var zero = aiPath.GetTransform(aiPath.GetLength()).MultiplyPoint3x4(Vector3.Zero);
            var expected = new Vector3(3, 0, 1);

            Assert.AreEqual(expected, zero);
            Assert.AreEqual(expected, end);
        }
        [TestMethod]
        public void TestEndTransform()
        {
            var loft = new Lofts.LinearPath(Vector3.Zero, new Vector3(0, 0, 10));

            var aiPath = new DummyAIPath(loft)
            {
                Reverse = false
            };
            aiPath.Offsets.Value = new PathOffsets(
                pathOffsetStart : 1,
                pathOffsetEnd : 2,
                sideOffsetStart : 3,
                sideOffsetEnd : 4);

            var end = aiPath.GetEndTransform().MultiplyPoint3x4(Vector3.Zero);
            var zero = aiPath.GetTransform(aiPath.GetLength()).MultiplyPoint3x4(Vector3.Zero);
            var expected = new Vector3(4, 0, 8);

            Assert.AreEqual(expected, zero);
            Assert.AreEqual(expected, end);
        }
    }

    internal class DummyAIPath : IAIPath
    {

        public DummyAIPath(LinearPath loft)
        {
            this.LoftPath = new BehaviorSubjectValue<ILoftPath>(loft);
             
        }

        public IObservableValue<ILoftPath> LoftPath { get; }

 

        public IAIPath LeftParralel => null;

        public IAIPath RightParralel => null;

        public bool Reverse { get; set; }

        public float MaxSpeed => 1;

        public float AverageSpeed => 1;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths.Value;
        public IObservableValue<IEnumerable<IAIPath>> NextPaths { get; } = new BehaviorSubjectValue<IEnumerable<IAIPath>>(Enumerable.Empty<IAIPath>());

        public LaneType LaneType => LaneType.Road;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public BehaviorSubjectValue<PathOffsets> Offsets { get; } = new BehaviorSubjectValue<PathOffsets>();

        IObservableValue<PathOffsets> IAIPath.Offsets => Offsets;
    }
}
