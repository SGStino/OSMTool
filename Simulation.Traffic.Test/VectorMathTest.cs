using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class VectorMathTest
    {
        [DataRow(new[] { -1f, 0, 0 }, new[] { 1f, 0, 0 }, Mathf.PI)]
        [DataRow(new[] { 2.5f, 0, -1.5f }, new[] { -2.5f, 0, 1.5f }, Mathf.PI)]
        [DataTestMethod]
        public void TestGetAngle180(float[] a, float[] b, float expected)
        {
            var from = new Vector3(a[0], a[1], a[2]);
            var to = new Vector3(b[0], b[1], b[2]);

            var angle = VectorMath3D.GetAngle(from, to, Vector3.up);
            Assert.AreEqual(expected, angle);
        }
    }
}
