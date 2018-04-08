using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data.Primitives;
using Simulation.Data.Trees;
using System.Numerics;

namespace Simulation.Data.Test
{
    [TestClass]
    public class QuadTreeUtilsTest
    {
        [TestMethod]
        public void TestEncapsulates()

        {
            var rectA = Rectangle.MinMaxRectangle(-1, -1, 2, 2);
            var rectB = Rectangle.MinMaxRectangle(0, 0, 1, 1);
            var rectC = Rectangle.MinMaxRectangle(-2, -2, 0, 0);
            var rectD = Rectangle.MinMaxRectangle(1, 1, 3, 3);

            Assert.IsTrue(QuadTreeUtils.Encapsulates(rectA, rectB));
            Assert.IsFalse(QuadTreeUtils.Encapsulates(rectA, rectC));
            Assert.IsFalse(QuadTreeUtils.Encapsulates(rectA, rectD));
        }

        [TestMethod]
        public void TestDevideQuads()
        {

            var root = Rectangle.MinMaxRectangle(0, 0, 2, 2);

            var required = new[]
            {
                Rectangle.MinMaxRectangle(0, 0, 1, 1),
                Rectangle.MinMaxRectangle(1, 0, 2, 1),
                Rectangle.MinMaxRectangle(0, 1, 1, 2),
                Rectangle.MinMaxRectangle(1, 1, 2, 2)
            };

            var subs = QuadTreeUtils.DevideQuads(root);

            CollectionAssert.AreEqual(required, subs);
        }


        [TestMethod]
        public void TestQuadIndex()
        {

            var root = Rectangle.MinMaxRectangle(-1, -1, 2, 2);
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
            var source = Rectangle.MinMaxRectangle(0, 0, 1, 1);

            var targets = new[]{
                new Vector2(0,0),
                new Vector2(1,0),
                new Vector2(0,1),
                new Vector2(1,1)
            };

            var required = new[]
            {
                Rectangle.MinMaxRectangle(-1, -1, 1, 1),
                Rectangle.MinMaxRectangle(0, -1, 2, 1),
                Rectangle.MinMaxRectangle(-1, 0, 1, 2),
                Rectangle.MinMaxRectangle(0,0, 2, 2)
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
