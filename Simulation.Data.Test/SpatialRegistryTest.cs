using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;
namespace Simulation.Data.Test
{
    [TestClass]
    public class SpatialRegistryTest
    {
        [TestMethod]
        public void TestQuery()
        {
            IList<string> data;

            var simpleSpatialRegistry = new SimpleSpatialRegistry<string>();

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(-1, -1, 2, 2)).ToList();
            Assert.AreEqual(0, data.Count);


            var reg = simpleSpatialRegistry.Register("topLeft", Rectangle.CornerSize(-0.75f, -0.75f, 0.5f, 0.5f));

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(-1, -1, 2, 2)).ToList();
            Assert.AreEqual("topLeft", data.Single());

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(0, 0, 1, 1)).ToList();
            Assert.AreEqual(0, data.Count);


            reg.Bounds = Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f);

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(-1, -1, 2, 2)).ToList();
            Assert.AreEqual("topLeft", data.Single());

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(0, 0, 1, 1)).ToList();
            Assert.AreEqual("topLeft", data.Single());


            reg.Dispose();

            data = simpleSpatialRegistry.Query(Rectangle.CornerSize(-1, -1, 2, 2)).ToList();
            Assert.AreEqual(0, data.Count);

        }

        [TestMethod]
        public void TestObserve()
        {

            var simpleSpatialRegistry = new SimpleSpatialRegistry<string>();

            var fullArea = simpleSpatialRegistry.Observe(Rectangle.CornerSize(-1, -1, 2, 2));
            var bottomRight = simpleSpatialRegistry.Observe(Rectangle.CornerSize(0, 0, 1, 1));


            var fullAreaQueue = new Queue<SpatialEvent<string>>();
            var bottomRightQueue = new Queue<SpatialEvent<string>>();
            fullAreaQueue.Enqueue(SpatialEvent<string>.Added("topLeft", Rectangle.CornerSize(-0.75f, -0.75f, 0.5f, 0.5f)));
            fullAreaQueue.Enqueue(SpatialEvent<string>.Moved("topLeft", Rectangle.CornerSize(-0.75f, -0.75f, 0.5f, 0.5f), Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f)));
            fullAreaQueue.Enqueue(SpatialEvent<string>.Removed("topLeft", Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f)));

            bottomRightQueue.Enqueue(SpatialEvent<string>.Moved("topLeft", Rectangle.CornerSize(-0.75f, -0.75f, 0.5f, 0.5f), Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f)));
            bottomRightQueue.Enqueue(SpatialEvent<string>.Removed("topLeft", Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f)));

            fullArea.Subscribe(t =>
            {
                Assert.AreEqual(t, fullAreaQueue.Dequeue());
            });

            bottomRight.Subscribe(t =>
            {
                Assert.AreEqual(t, bottomRightQueue.Dequeue());
            });


            var reg = simpleSpatialRegistry.Register("topLeft", Rectangle.CornerSize(-0.75f, -0.75f, 0.5f, 0.5f));

            reg.Bounds = Rectangle.CornerSize(0.25f, 0.25f, 0.5f, 0.5f);

            reg.Dispose();

            Assert.AreEqual(0, fullAreaQueue.Count);
            Assert.AreEqual(0, bottomRightQueue.Count);
        }
    }
}
