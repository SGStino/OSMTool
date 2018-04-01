using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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

            data = simpleSpatialRegistry.Query(new Rect(-1, -1, 2, 2)).ToList();
            Assert.AreEqual(0, data.Count);


            var reg = simpleSpatialRegistry.Register("topLeft", new Rect(-0.75f, -0.75f, 0.5f, 0.5f));

            data = simpleSpatialRegistry.Query(new Rect(-1, -1, 2, 2)).ToList();
            Assert.AreEqual("topLeft", data.Single());

            data = simpleSpatialRegistry.Query(new Rect(0, 0, 1, 1)).ToList();
            Assert.AreEqual(0, data.Count);


            reg.Bounds = new Rect(0.25f, 0.25f, 0.5f, 0.5f);

            data = simpleSpatialRegistry.Query(new Rect(-1, -1, 2, 2)).ToList();
            Assert.AreEqual("topLeft", data.Single());

            data = simpleSpatialRegistry.Query(new Rect(0, 0, 1, 1)).ToList();
            Assert.AreEqual("topLeft", data.Single());


            reg.Dispose();

            data = simpleSpatialRegistry.Query(new Rect(-1, -1, 2, 2)).ToList();
            Assert.AreEqual(0, data.Count);

        }

        [TestMethod]
        public void TestObserve()
        {

            var simpleSpatialRegistry = new SimpleSpatialRegistry<string>();

            var fullArea = simpleSpatialRegistry.Observe(new Rect(-1, -1, 2, 2));
            var bottomRight = simpleSpatialRegistry.Observe(new Rect(0, 0, 1, 1));


            var fullAreaQueue = new Queue<SpatialEvent<string>>();
            var bottomRightQueue = new Queue<SpatialEvent<string>>();
            fullAreaQueue.Enqueue(SpatialEvent<string>.Added("topLeft", new Rect(-0.75f, -0.75f, 0.5f, 0.5f)));
            fullAreaQueue.Enqueue(SpatialEvent<string>.Moved("topLeft", new Rect(-0.75f, -0.75f, 0.5f, 0.5f), new Rect(0.25f, 0.25f, 0.5f, 0.5f)));
            fullAreaQueue.Enqueue(SpatialEvent<string>.Removed("topLeft", new Rect(0.25f, 0.25f, 0.5f, 0.5f)));

            bottomRightQueue.Enqueue(SpatialEvent<string>.Moved("topLeft", new Rect(-0.75f, -0.75f, 0.5f, 0.5f), new Rect(0.25f, 0.25f, 0.5f, 0.5f)));
            bottomRightQueue.Enqueue(SpatialEvent<string>.Removed("topLeft", new Rect(0.25f, 0.25f, 0.5f, 0.5f)));

            fullArea.Subscribe(t =>
            {
                Assert.AreEqual(t, fullAreaQueue.Dequeue());
            });

            bottomRight.Subscribe(t =>
            {
                Assert.AreEqual(t, bottomRightQueue.Dequeue());
            });


            var reg = simpleSpatialRegistry.Register("topLeft", new Rect(-0.75f, -0.75f, 0.5f, 0.5f));

            reg.Bounds = new Rect(0.25f, 0.25f, 0.5f, 0.5f);

            reg.Dispose();

            Assert.AreEqual(0, fullAreaQueue.Count);
            Assert.AreEqual(0, bottomRightQueue.Count);
        }
    }
}
