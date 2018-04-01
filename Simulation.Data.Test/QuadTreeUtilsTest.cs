using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data.Trees;
using UnityEngine;

namespace Simulation.Data.Test
{
    [TestClass]
    public class QuadTreeUtilsTest
    {
        [TestMethod]
        public void TestEncapsulates()

        {
            var rectA = Rect.MinMaxRect(-1, -1, 2, 2);
            var rectB = Rect.MinMaxRect(0, 0, 1, 1);
            var rectC = Rect.MinMaxRect(-2, -2, 0, 0);
            var rectD = Rect.MinMaxRect(1, 1, 3, 3);

            Assert.IsTrue(QuadTreeUtils.Encapsulates(rectA, rectB));
            Assert.IsFalse(QuadTreeUtils.Encapsulates(rectA, rectC));
            Assert.IsFalse(QuadTreeUtils.Encapsulates(rectA, rectD));
        }

        [TestMethod]
        public void TestDevideQuads()
        {

            var root = Rect.MinMaxRect(0, 0, 2, 2);

            var required = new[]
            {
                Rect.MinMaxRect(0, 0, 1, 1),
                Rect.MinMaxRect(1, 0, 2, 1),
                Rect.MinMaxRect(0, 1, 1, 2),
                Rect.MinMaxRect(1, 1, 2, 2)
            };

            var subs = QuadTreeUtils.DevideQuads(root);

            CollectionAssert.AreEqual(required, subs);
        }


        [TestMethod]
        public void TestQuadIndex()
        {

            var root = Rect.MinMaxRect(-1, -1, 2, 2);
            var targets = new[]{
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };

            for (int i = 0; i < 4; i++)
            {
                int i2 = QuadTreeUtils.GetQuadIndex(root, targets[i]);
                Assert.AreEqual(i, i2);
            }
        }


        [TestMethod]
        public void TestGrowQuad()
        {
            var source = Rect.MinMaxRect(0, 0, 1, 1);

            var targets = new[]{
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };

            var required = new[]
            {
                Rect.MinMaxRect(-1, -1, 1, 1),
                Rect.MinMaxRect(0, -1, 2, 1),
                Rect.MinMaxRect(-1, 0, 1, 2),
                Rect.MinMaxRect(0,0, 2, 2)
            };

            var sourceIndices = new[]
            {
                3,2,1,0
            };

            for (int i = 0; i < 4; i++)
            {
                var quad = QuadTreeUtils.GrowQuad(source, targets[i], out var i2, out var i3);
                Assert.AreEqual(i, i2);
                Assert.AreEqual(required[i], quad, $"quadrant {i}");
                Assert.AreEqual(sourceIndices[i], i3);
            }
        }
    }
}
