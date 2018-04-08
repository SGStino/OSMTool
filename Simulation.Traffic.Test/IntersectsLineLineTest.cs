using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class IntersectsLineLineTest
    {
        [TestMethod]
        public void TestIntersectLineLineCommonPointStart()
        {
            var a = Vector2.One;
            var b = Directions2.Right;
            var c = Directions2.Up;

            VectorMath2D.IntersectsLineLine(a, -b, a, -c, out var aL, out var bL);

            Assert.AreEqual(0, aL);
            Assert.AreEqual(0, bL);
        }
        [TestMethod]
        public void TestIntersectLineLineCommonPointEnd()
        {
            var a = Vector2.One;
            var b = Directions2.Right;
            var c = Directions2.Up;

            VectorMath2D.IntersectsLineLine(a + b, -b, a + c, -c, out var aL, out var bL);

            Assert.AreEqual(1, aL);
            Assert.AreEqual(1, bL);
        }
        [TestMethod]
        public void TestIntersectLineLineDoubleEnd()
        {
            var a = Vector2.One;
            var b = Directions2.Right;
            var c = Directions2.Up;

            VectorMath2D.IntersectsLineLine(a + b * 2, -b, a + c * 2, -c, out var aL, out var bL);

            Assert.AreEqual(2, aL);
            Assert.AreEqual(2, bL);
        }
        [DataTestMethod]

        [DataRow(1, 1, 1, 1)]
        [DataRow(-1, 1, 1, 1)]
        [DataRow(1, -1, 1, 1)]
        [DataRow(-1, -1, 1, 1)]
        public void TestIntersectLineLineDoubleEndWithWidth(float tB, float tC, float eAL, float eBL)
        {
            var a = Vector2.One;
            var b = Directions2.Right * tB;
            var c = Directions2.Up * tC;

            var w = 2;

            VectorMath2D.IntersectsLineLine(a + b * 2, -b, w, a + c * 2, -c, w, out var aL, out var bL);

            Assert.AreEqual(eAL, aL);
            Assert.AreEqual(eBL, bL);
        }

        [TestMethod]
        public void TestIntersectLineLineInequalWidth()
        {
            var a = Vector2.Zero;
            var b = Directions2.Right;
            var c = Directions2.Up;

            var w1 = 2;
            var w2 = 4;

            VectorMath2D.IntersectsLineLine(a, b, w1, a, c, w2, out var aL, out var bL);
             
        }




        [TestMethod]
        public void TestIntersectLineLineSweep()
        {

            int count = 10;
            float delta = MathF.PI * 2 / count;

            for (int iA = -count; iA < count; iA++)
            {
                var angleA = delta * iA;
                for (int iB = -count; iB < count; iB++)
                {
                    var angleB = delta * iB;
                    TestAngle(angleA, angleB);

                }
            }

        }

        [DataTestMethod]
        [DataRow(-5.654867f, -2.51327419f)]
        public void TestAngle(float angleA, float angleB)
        {
            Vector2 point = Vector2.Zero;
            var vA = new Vector2(MathF.Cos(angleA), MathF.Sin(angleA));
            var deltaAngle = (Math.Max(angleA, angleB) - Math.Min(angleA, angleB)) % (MathF.PI * 2);
            var vB = new Vector2(MathF.Cos(angleB), MathF.Sin(angleB));

            var halfAngle = deltaAngle / 2;

            var result = VectorMath2D.IntersectsLineLine(point, vA, 2, point, vB, 2, out var aL, out var bL);

            var d = MathF.Abs(1 / MathF.Tan(halfAngle));
            if (almostEqual(deltaAngle, MathF.PI))
            {
                Assert.IsFalse(result);
            }
            else if (almostEqual(deltaAngle, 0) || almostEqual(deltaAngle, MathF.PI * 2))
            {
                Assert.IsTrue(float.IsInfinity(aL));
                Assert.IsTrue(float.IsInfinity(bL));
                Assert.IsFalse(result);
            }
            else
            {
                Assert.AreEqual(d, aL, 0.0001f);
                Assert.AreEqual(d, bL, 0.0001f);
                Assert.IsTrue(result);
            }
        }

        private bool almostEqual(float angleA, float angleB, float maxDelta = 0.0001f)
        {
            var delta = MathF.Abs(angleA - angleB);
            return delta <= maxDelta;
        }
    }
}
