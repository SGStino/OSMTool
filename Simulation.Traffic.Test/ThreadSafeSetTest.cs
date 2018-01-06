using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class ThreadSafeSetTest
    {
        [TestMethod]
        public void TestAdd()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());


            var valueA = "a";
            Assert.IsTrue(testSet.Add(valueA));
            Assert.IsFalse(testSet.Add(valueA));
        }

        [TestMethod]
        public void TestClear()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());


            testSet.Add("a");
            testSet.Add("b");
            Assert.IsTrue(testSet.Count > 0, "count before clear");
            testSet.Clear();
            Assert.IsFalse(testSet.Count > 0, "count after clear");
        }

        [TestMethod]
        public void TestContains()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());


            testSet.Add("a");
            Assert.IsTrue(testSet.Contains("a"), "contains valid");
            Assert.IsFalse(testSet.Contains("b"), "contains invalid");
        }
        [TestMethod]
        public void TestCopyTo()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());


            testSet.Add("a");

            string[] array = new string[1];
            testSet.CopyTo(array, 0);
            Assert.AreEqual("a", array[0]);
            Assert.AreEqual(1, array.Length);
        }
        [TestMethod]
        public void TestExceptWith()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "b" };

            testSet.Add("a");
            testSet.Add("b");

            testSet.ExceptWith(bSet);

            Assert.AreEqual("a", testSet.Single());

            testSet.ExceptWith(bSet);

            Assert.AreEqual("a", testSet.Single());
        }
        [TestMethod]
        public void TestSymmetricExceptWith()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "b" };

            testSet.Add("a");
            testSet.Add("b");

            testSet.SymmetricExceptWith(bSet);

            Assert.AreEqual("a", testSet.Single());

            testSet.SymmetricExceptWith(bSet);

            Assert.AreEqual(2, testSet.Count);
        }
        [TestMethod]
        public void TestIntersectWith()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "b" };

            testSet.Add("a");
            testSet.Add("b");

            testSet.IntersectWith(bSet);

            Assert.AreEqual("b", testSet.Single());

            testSet.IntersectWith(bSet);

            Assert.AreEqual("b", testSet.Single());
        }
        [TestMethod]
        public void TestIsProperSubsetOf()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "a", "b", "c" };
            var dSet = new[] { "b", "c", "d" };
            testSet.Add("a");
            testSet.Add("b");

            Assert.IsTrue(testSet.IsProperSubsetOf(bSet));
            Assert.IsFalse(testSet.IsProperSubsetOf(dSet));
        }
        [TestMethod]
        public void TestIsProperSupersetOf()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "a", "b", "c" };
            var dSet = new[] { "b", "c", "d" };
            testSet.Add("a");
            testSet.Add("b");
            testSet.Add("c");
            testSet.Add("e");

            Assert.IsTrue(testSet.IsProperSupersetOf(bSet));
            Assert.IsFalse(testSet.IsProperSupersetOf(dSet));
        }
        [TestMethod]
        public void TestIsSubsetOf()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "a", "b", "c" };
            var dSet = new[] { "b", "c", "d" };
            testSet.Add("a");
            testSet.Add("b");

            Assert.IsTrue(testSet.IsSubsetOf(bSet));
            Assert.IsFalse(testSet.IsSubsetOf(dSet));
        }
        [TestMethod]
        public void TestIsSupersetOf()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "a", "b", "c" };
            var dSet = new[] { "b", "c", "d" };
            testSet.Add("a");
            testSet.Add("b");
            testSet.Add("c"); 

            Assert.IsTrue(testSet.IsSupersetOf(bSet));
            Assert.IsFalse(testSet.IsSupersetOf(dSet));
        }
        [TestMethod]
        public void TestOverlaps()
        {
            var testSet = new ThreadSafeSet<string>(new HashSet<string>());

            var bSet = new[] { "a", "b", "c" };
            var dSet = new[] { "b", "c", "d" };
            testSet.Add("a");
            testSet.Add("b");
            testSet.Add("c");

            Assert.IsTrue(testSet.Overlaps(bSet));
            Assert.IsTrue(testSet.Overlaps(dSet));
        }
    }
}
