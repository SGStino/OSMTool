﻿

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

        public void SnapTo(Vector3 to, out float distance)
        {
            VectorMath3D.GetPointOnSegment(start, dir, length, to, out distance);
        }

        public bool Intersects(Plane plane, out float[] loftDistances)
        {
            var ray = new Ray(start, dir);

            if (plane.Raycast(ray, out float dist))
            {
                loftDistances = new[] { dist };
                return true;
            }
            loftDistances = new float[0];
            return false;
        }

        public Rect GetBounds(float width)
        { 

            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;


            var points = new[] { 0f, 1f };



            for (int i = 0; i < 2; i++)
            {
                var m = GetTransform(points[i] * length);
                var offset = m.GetRight() * width;
                var point = m.GetTranslate();

                var min = point - offset;
                var max = point + offset;

                minX = Mathf.Min(minX, min.x, max.x);
                minZ = Mathf.Min(minZ, min.z, max.z);
                maxX = Mathf.Max(maxX, max.x, max.x);
                maxZ = Mathf.Max(maxZ, max.z, max.z);
            }

            return Rect.MinMaxRect(minX, minZ, maxX, maxZ);
        }
    }
}
