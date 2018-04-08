

using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

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
            if (float.IsNaN(start.X)) throw new InvalidOperationException("start.X is not a number");
            if (float.IsNaN(start.Y)) throw new InvalidOperationException("start.Y is not a number");
            if (float.IsNaN(start.Z)) throw new InvalidOperationException("start.Z is not a number");
            if (float.IsNaN(end.X)) throw new InvalidOperationException("end.X is not a number");
            if (float.IsNaN(end.Y)) throw new InvalidOperationException("end.Y is not a number");
            if (float.IsNaN(end.Z)) throw new InvalidOperationException("end.Z is not a number");
#endif

            var dir = (end - start);
            this.length = dir.Length();
            this.start = start;
            this.dir = dir / length;
            this.tangent = Vector3.Normalize(Vector3.Cross(this.dir, Directions3.Up));
        }

        public float Length { get { return length; } }

        public Matrix4x4 GetTransform(float progress)
        {
            var position = this.start + this.dir * progress;

            var side = tangent;

            var forward = dir;

            side = Vector3.Normalize(Vector3.Cross(Directions3.Up, forward));

            var up = Vector3.Cross(forward, side);


            Matrix4x4 matrix = Matrix4x4.Identity;
            matrix.SetColumn(0, side);
            matrix.SetColumn(1, up);
            matrix.SetColumn(2, forward);
            matrix.SetColumn(3, new Vector4(position.X, position.Y, position.Z, 1));
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

        public Rectangle GetBounds(float width)
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

                minX = MathF.Min(minX, min.X, max.X);
                minZ = MathF.Min(minZ, min.Z, max.Z);
                maxX = MathF.Max(maxX, max.X, max.X);
                maxZ = MathF.Max(maxZ, max.Z, max.Z);
            }

            return Rectangle.MinMaxRectangle(minX, minZ, maxX, maxZ);
        }
    }
}
