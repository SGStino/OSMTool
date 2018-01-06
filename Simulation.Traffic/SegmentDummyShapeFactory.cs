using System;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using UnityEngine;
using System.Linq;

namespace Simulation.Traffic
{
    internal class SegmentDummyShapeFactory : IComponentValueFactory<ILoftShape, Segment>
    {
        public static SegmentDummyShapeFactory Default { get; } = new SegmentDummyShapeFactory();

        public SegmentDummyShapeFactory()
        {
        }

        public Task<ILoftShape> Create(Segment segment, CancellationToken cancel)
        {
            return Task.Run(() => CreateShape(segment, cancel), cancel);
        }

        private ILoftShape CreateShape(Segment segment, CancellationToken cancel)
        {
            var description = segment.Description;
            var width = description.Lanes.Sum(l => l.Width);
            int indexCount = description.Lanes.Length * 2;
            int vertexCount = description.Lanes.Length + 1;

            var left = -width / 2;
            var vertices = new LoftVertex[vertexCount];
            var indices = new int[indexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                cancel.ThrowIfCancellationRequested();
                LoftVertex v;
                v.Position = new Vector3(left, 0, 0);
                v.Normal = Vector3.up;
                v.U = left / width + 0.5f;
                vertices[i] = v;
                if (i < description.Lanes.Length)
                    left += description.Lanes[i].Width;
                if (i > 0)
                {
                    indices[i * 2 - 2] = i - 1;
                    indices[i * 2 - 1] = i;
                }
            }
            return new ArrayLoftShape(indices, vertices, 1.0f / width);
        }
    }
}