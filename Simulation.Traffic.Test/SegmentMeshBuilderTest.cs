using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class SegmentMeshBuilderTest
    {
        [TestMethod]
        public void TestMeshBuilder()
        {
            var loftPath = new LinearPath(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
            var description = new SegmentShape()
            {
                Indices = new int[] { 0, 1, 2, 3 },
                Vertices = new Vector2[] { new Vector2(-2, 0), new Vector2(-1, 0), new Vector2(1, 0), new Vector2(2, 0) },
                Normals = new Vector2[] { new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1) },
                Texcoords = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(1, 1) }
            };
            var mesh = SegmentMeshBuilder.BuildMesh(loftPath, description);

            var quads = mesh.Indices.Length / 6;

            for (int q = 0; q < quads; q++)
            {
                var quad = mesh.Indices.Skip(q * 6).Take(6).OrderBy(i => i).ToArray();
                Assert.AreEqual(6, quad.Length);
                Assert.AreEqual(4, quad.Distinct().Count());


                var high = quad.Skip(3).Take(3);
                var low = quad.Take(3);

                Assert.IsTrue(high.SequenceEqual(low.Select(i => i + 4)));

                var vertices = mesh.Indices.Skip(q * 6).Take(6).Select(i => mesh.Positions[i]).ToArray();



                var n1 = Vector3.Cross(vertices[0] - vertices[1], vertices[2] - vertices[1]);
                Assert.AreEqual(1, MathF.Sign(Vector3.Dot(n1, new Vector3(0, 1, 0))));
                var n2 = Vector3.Cross(vertices[3] - vertices[4], vertices[5] - vertices[4]);
                Assert.AreEqual(1, MathF.Sign(Vector3.Dot(n2, new Vector3(0, 1, 0))));

            }

        }
    }
}
