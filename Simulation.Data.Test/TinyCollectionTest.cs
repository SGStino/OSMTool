using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Data.Trees;

namespace Simulation.Data.Test
{
    [TestClass]
    public class TinyCollectionTest
    {
        [TestMethod]
        public void TestAddRemove()
        {
            var collection = new TinyItemCollection<string>();

            Assert.IsTrue(collection.Add("hello"));
            Assert.IsTrue(collection.Add("world"));
            Assert.IsTrue(collection.Add("!"));
            Assert.IsFalse(collection.Add("world"));

            Assert.IsTrue(collection.Remove("world"));
            Assert.IsFalse(collection.Remove("world"));

        }
    }
}
