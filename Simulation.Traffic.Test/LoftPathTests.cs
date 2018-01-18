using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Utilities;
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
        public void TestPositiveArc()
        {
            var arc = new ArcLoftPath(Vector3.left, Mathf.PI / 2, Vector3.zero, Vector3.up);

            Assert.AreEqual(Mathf.PI / 2, arc.Length);
            var startTransform = arc.GetTransform(0);

            var endTransform = arc.GetTransform(arc.Length);

            var startForward = startTransform.MultiplyVector(Vector3.forward);
            var endForward = endTransform.MultiplyVector(Vector3.forward);

            var startPosition = startTransform.MultiplyPoint3x4(Vector3.zero);
            var endPosition = endTransform.MultiplyPoint3x4(Vector3.zero);

            Assert.AreEqual(Vector3.left, startPosition.Round(0.00001f));
            Assert.AreEqual(Vector3.forward, endPosition.Round(0.00001f));

            Assert.AreEqual(Vector3.forward, startForward.Round(0.00001f));
            Assert.AreEqual(Vector3.right, endForward.Round(0.00001f));

            var halfTransform = arc.GetTransform(arc.Length / 2);


            var halfForward = new Vector3(1, 0, 1).normalized;

            Assert.AreEqual(halfForward.Round(0.00001f), halfTransform.MultiplyVector(Vector3.forward).Round(0.00001f));

        }

        [TestMethod]
        public void TestNegativeArc()
        {
            var arc = new ArcLoftPath(Vector3.left, -Mathf.PI / 2, Vector3.zero, Vector3.up);

            Assert.AreEqual(Mathf.PI / 2, arc.Length);
            var startTransform = arc.GetTransform(0);

            var endTransform = arc.GetTransform(arc.Length);

            var startForward = startTransform.MultiplyVector(Vector3.forward);
            var endForward = endTransform.MultiplyVector(Vector3.forward);

            var startPosition = startTransform.MultiplyPoint3x4(Vector3.zero);
            var endPosition = endTransform.MultiplyPoint3x4(Vector3.zero);

            Assert.AreEqual(Vector3.left, startPosition.Round(0.00001f));
            Assert.AreEqual(Vector3.back, endPosition.Round(0.00001f));

            Assert.AreEqual(Vector3.back, startForward.Round(0.00001f));
            Assert.AreEqual(Vector3.right, endForward.Round(0.00001f));

            var halfTransform = arc.GetTransform(arc.Length / 2);


            var halfForward = new Vector3(1, 0, -1).normalized;

            Assert.AreEqual(halfForward.Round(0.00001f), halfTransform.MultiplyVector(Vector3.forward).Round(0.00001f));

        }



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
