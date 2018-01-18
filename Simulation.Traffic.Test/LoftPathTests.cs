using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class LoftPathTests
    {
        [TestMethod]
        public void TestLinearX()
        { 
            var linearLoftPath = new LinearPath(Vector3.zero, new Vector3(1, 0, 0));

            var transformStart = linearLoftPath.GetTransform(0);
            var transformEnd = linearLoftPath.GetTransform(1);

            var forwardStart = transformStart.MultiplyVector(Vector3.forward);

            var forwardEnd = transformEnd.MultiplyVector(Vector3.forward);

            Assert.AreEqual(Vector3.right, forwardStart);
            Assert.AreEqual(Vector3.right, forwardEnd);


            var rightStart = transformStart.MultiplyVector(Vector3.right);

            var rightEnd = transformEnd.MultiplyVector(Vector3.right);
            Assert.AreEqual(Vector3.back, rightStart);
            Assert.AreEqual(Vector3.back, rightEnd);
        }


        [TestMethod]
        public void TestLinearZ()
        {
            var linearLoftPath = new LinearPath(Vector3.zero, new Vector3(0, 0, 1));

            var transformStart = linearLoftPath.GetTransform(0);
            var transformEnd = linearLoftPath.GetTransform(1);

            var forwardStart = transformStart.MultiplyVector(Vector3.forward);

            var forwardEnd = transformEnd.MultiplyVector(Vector3.forward);

            Assert.AreEqual(Vector3.forward, forwardStart);
            Assert.AreEqual(Vector3.forward, forwardEnd);


            var rightStart = transformStart.MultiplyVector(Vector3.right);

            var rightEnd = transformEnd.MultiplyVector(Vector3.right);
            Assert.AreEqual(Vector3.right, rightStart);
            Assert.AreEqual(Vector3.right, rightEnd);
        }
    }
}
