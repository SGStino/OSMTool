using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
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
    public class AIPathTest
    {
        [TestMethod]
        public void TestExtend()
        {

            var linear = new LinearPath(Vector3.zero, Vector3.forward * 10);


            var start = new DummyAIPath(linear);

            var array = new IAIPath[] { start };

            Matrix4x4 to = LookAt(Vector3.forward * 10 + Vector3.left * 5, Vector3.forward * 12 + Vector3.left * 15, Vector3.up);

            Matrix4x4 from = LookAt(Vector3.right * 5, Vector3.forward * 2.5f, Vector3.up);

            var extended = array.Extend(from, to);

            Assert.AreEqual(3, extended.Length);

            Assert.AreEqual(0f, extended[0].Start);
            Assert.AreEqual(1f, extended[0].End);

            Assert.AreEqual(0f, extended[2].Start);
            Assert.AreEqual(1f, extended[2].End);



            Assert.AreEqual(2.5f, extended[1].Start, 0.0001f);
            Assert.AreEqual(9f, extended[1].End, 0.0001f);

            var t0Start = extended[0].Path.GetTransform(extended[0].Start);
            var t0End = extended[0].Path.GetTransform(extended[0].End);
            var t1Start = extended[1].Path.GetTransform(extended[1].Start);
            var t1End = extended[1].Path.GetTransform(extended[1].End);
            var t2Start = extended[2].Path.GetTransform(extended[2].Start);
            var t2End = extended[2].Path.GetTransform(extended[2].End);

            Equal(from, t0Start);
        }

        private void Equal(Matrix4x4 a, Matrix4x4 b)
        {
            var tA = a.GetTranslate();
            var tB = b.GetTranslate();


            Equal(tA, tB);
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    Assert.AreEqual(a[x, y], b[x, y], 0.00001f, "m" + x + y);
        }

        private void Equal(Vector3 tA, Vector3 tB)
        {
            for (int x = 0; x < 3; x++)
                Assert.AreEqual(tA[x], tB[x], 0.00001f, "v" + x);
        }

        private Matrix4x4 LookAt(Vector3 a, Vector3 b, Vector3 up)
        {
            var translate = a;
            var forward = (b - a).normalized;
            var right = Vector3.Cross(forward, up);

            Matrix4x4 m = Matrix4x4.identity;
            m.SetColumn(0, right);
            m.SetColumn(1, up);
            m.SetColumn(2, forward);
            m.SetColumn(3, new Vector4(translate.x, translate.y, translate.z, 1));

            return m;
        }
    }
}
