using System;
using System.Collections.Generic;
using System.Numerics;

namespace Simulation.Rendering
{
    public struct MeshData
    {
        public readonly Vector3[] Positions;
        public readonly Vector3[] Normals;
        public readonly Vector4[] Tangents;
        public readonly Vector2[] Texcoords;
        public readonly int[] Indices;

        public MeshData(int vertexCount, int indexCount)
        {
            Positions = new Vector3[vertexCount];
            Normals = new Vector3[vertexCount];
            Tangents = new Vector4[vertexCount];
            Texcoords = new Vector2[vertexCount];
            Indices = new int[indexCount];
        }

        public MeshData(List<Vector3> positions, List<Vector3> normals, List<Vector4> tangents, List<Vector2> texcoords, List<int> indices)
        {
            Positions = positions.ToArray();
            Normals = normals.ToArray();
            Tangents = tangents.ToArray();
            Texcoords = texcoords.ToArray();
            Indices = indices.ToArray();
        }

 


        public static MeshData Merge(params MeshData[] data) => Merge((IReadOnlyList<MeshData>)data);
        public static MeshData Merge(IReadOnlyList<MeshData> data)
        {
            var vertexCount = 0;
            var indexCount = 0;
            var vertexOffsets = new int[data.Count];
            var indexOffsets = new int[data.Count];

            for (int i = 0; i < data.Count; i++)
            {
                indexOffsets[i] = indexCount;
                vertexOffsets[i] = vertexCount;
                vertexCount += data[i].Positions.Length;
                indexCount += data[i].Indices.Length;
            }


            var newMesh = new MeshData(vertexCount, indexCount);
            for (int i = 0; i < data.Count; i++)
                copyTo(data[i], newMesh, vertexOffsets[i], indexOffsets[i]);
            return newMesh;
        }

        private static void copyTo(MeshData data, MeshData newMesh, int vOffset, int iOffset)
        {
            copyTo(data.Positions, newMesh.Positions, vOffset);
            copyTo(data.Normals, newMesh.Normals, vOffset);
            copyTo(data.Tangents, newMesh.Tangents, vOffset);
            copyTo(data.Texcoords, newMesh.Texcoords, vOffset);

            copyTo(data.Indices, newMesh.Indices, iOffset, vOffset);
        }

        private static void copyTo(Array source, Array target, int offset)
        {
            Array.Copy(source, 0, target, offset, source.Length);
        }

        private static void copyTo(int[] source, int[] target, int offset, int add)
        {
            for (int i = 0; i < source.Length; i++)
                target[offset + i] = source[i] + add;
        }
    }
}
