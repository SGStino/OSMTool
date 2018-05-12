using Simulation.Rendering;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reactive.Linq;
using System.Text;
using System.Linq;
using Simulation.Data;

namespace Simulation.Traffic
{
    public static class SegmentMeshBuilder
    {
        public static IObservableValue<MeshData> BuildMesh(this ISegment segment) =>
            segment.LoftPath.Select(p => BuildMeshAsync(p, BuildShape(segment.Description))).Switch().ToObservableValue();

        public static SegmentShape BuildShape(SegmentDescription description)
        {
            var vertCount = description.Lanes.Length * 2;
            var vertices = new List<Vector2>(vertCount);
            var texcoords = new List<Vector2>(vertCount);
            var normals = new List<Vector2>(vertCount);
            var indices = new List<int>(vertCount);

            var width = description.Lanes.Sum(n => n.Width);
            var offset = -width / 2;
            int i = 0;
            foreach (var segment in description.Lanes)
            {
                var v1 = new Vector2(offset, 0);
                offset += segment.Width;
                var v2 = new Vector2(offset, 0);
                vertices.Add(v1);
                vertices.Add(v2);
                normals.Add(new Vector2(0, 1));
                normals.Add(new Vector2(0, 1));
                var vScale = 1 / segment.Width;
                if (!segment.Reverse)
                {
                    texcoords.Add(new Vector2(0, vScale));
                    texcoords.Add(new Vector2(1, vScale));
                }
                else
                {
                    texcoords.Add(new Vector2(1, -vScale));
                    texcoords.Add(new Vector2(0, -vScale));
                }
                indices.Add(i++);
                indices.Add(i++);
            }
            return new SegmentShape()
            {
                Vertices = vertices.ToArray(),
                Texcoords = texcoords.ToArray(),
                Indices = indices.ToArray(),
                Normals = normals.ToArray()
            };
        }

        public static IObservable<MeshData> BuildMeshAsync(ILoftPath p, SegmentShape description)
        {
            // TODO: schedule on worker thread
            return Observable.Return(BuildMesh(p, description));
        }
        public static MeshData BuildMesh(ILoftPath p, SegmentShape description)
        {
            // TODO: move code to loftbuilder
            var count = MathF.CeilToInt(p.Length) / 10; // every 10 meter

            var vertexStride = description.Vertices.Length;
            var indexStride = description.Indices.Length * 3;


            var m = new MeshData((count + 1) * vertexStride, count * indexStride);


            var delta = p.Length / count;
            for (int l = 0; l <= count; l++)
            {
                int v = l * vertexStride;
                int i = (l - 1) * indexStride;

                var distance = MathF.Clamp(0, p.Length, l * delta);
                var transform = p.GetTransform(distance);

                for (int vO = 0; vO < vertexStride; vO++)
                {
                    var vIn = description.Vertices[vO];
                    var nIn = description.Normals[vO];
                    var tIn = description.Texcoords[vO];

                    var pos = new Vector3(vIn, 0);
                    m.Positions[v + vO] = Vector3.Transform(pos, transform);
                    var normal = new Vector3(nIn, 0);
                    m.Normals[v + vO] = Vector3.TransformNormal(normal, transform);
                    m.Tangents[v + vO] = new Vector4(Vector3.TransformNormal(new Vector3(0, 0, 1), transform), 1); // check with UV!

                    m.Texcoords[v + vO] = new Vector2(tIn.X, tIn.Y * distance);
                }
                if (l > 0)
                    for (int iO = 0; iO < description.Indices.Length; iO += 2)
                    {
                        var iT = i + iO * 3;
                        var iIn0 = description.Indices[iO];
                        var iIn1 = description.Indices[iO + 1];

                        m.Indices[iT/**/] = iIn0 + v;
                        m.Indices[iT + 1] = iIn0 + v - vertexStride; // previous layer
                        m.Indices[iT + 2] = iIn1 + v;
                        m.Indices[iT + 3] = iIn1 + v;
                        m.Indices[iT + 4] = iIn0 + v - vertexStride; // previous layer
                        m.Indices[iT + 5] = iIn1 + v - vertexStride; // previous layer
                    }
            }
            return m;
        }
    }

    public struct SegmentShape
    {
        public Vector2[] Normals { get; set; }
        public Vector2[] Vertices { get; set; }
        public Vector2[] Texcoords { get; set; }
        public int[] Indices { get; set; }
    }
}
