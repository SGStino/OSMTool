using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Utilities;
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
    public class VectorMathTest
    {
        [DataRow(new[] { -1f, 0, 0 }, new[] { 1f, 0, 0 }, MathF.PI)]
        [DataRow(new[] { 2.5f, 0, -1.5f }, new[] { -2.5f, 0, 1.5f }, MathF.PI)]
        [DataTestMethod]
        public void TestGetAngle180(float[] a, float[] b, float expected)
        {
            var from = new Vector3(a[0], a[1], a[2]);
            var to = new Vector3(b[0], b[1], b[2]);

            var angle = VectorMath3D.GetAngle(from, to, Directions3.Up);
            Assert.AreEqual(expected, angle);
        }

        [TestMethod]
        public void TestMatrix()
        {
            var identity = Matrix4x4.CreateWorld(Vector3.Zero, Directions3.Forward, Directions3.Up);

            Assert.IsTrue(identity.IsIdentity);
        }
    }
}
