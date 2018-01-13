

using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public class LinearPath : ILoftPath
    {
        private readonly Vector3 dir;
        private readonly float length;
        private readonly Vector3 start;
        private readonly Vector3 tangent;

        public Vector3 Start { get { return start; } }
        public Vector3 Tangent { get { return tangent; } }
        public Vector3 Dir { get { return dir; } }

        public LinearPath(Vector3 start, Vector3 end)
        {
#if DEBUG
            if (float.IsNaN(start.x)) throw new InvalidOperationException("start.x is not a number");
            if (float.IsNaN(start.y)) throw new InvalidOperationException("start.y is not a number");
            if (float.IsNaN(start.z)) throw new InvalidOperationException("start.z is not a number");
            if (float.IsNaN(end.x)) throw new InvalidOperationException("end.x is not a number");
            if (float.IsNaN(end.y)) throw new InvalidOperationException("end.y is not a number");
            if (float.IsNaN(end.z)) throw new InvalidOperationException("end.z is not a number");
#endif

            var dir = (end - start);
            this.length = dir.magnitude;
            this.start = start;
            this.dir = dir / length;
            this.tangent = Vector3.Cross(this.dir, Vector3.up).normalized;
        }

        public float Length { get { return length; } }

        public Matrix4x4 GetTransform(float progress)
        {
            var position = this.start + this.dir * progress;

            var side = tangent;

            var forward = dir;

            side = Vector3.Cross(Vector3.up, forward).normalized;

            var up = Vector3.Cross(forward, side);


            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetColumn(0, side);
            matrix.SetColumn(1, up);
            matrix.SetColumn(2, forward);
            matrix.SetColumn(3, new Vector4(position.x, position.y, position.z, 1));
            return matrix;
        }

        public void SnapTo(Vector3 to, out Vector3 position, out float distance)
        {
            position = VectorMath3D.GetPointOnSegment(start, dir, length, to);
            distance = (position - to).magnitude;
        }
    }
}
