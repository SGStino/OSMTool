using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public static class VectorExtensions
    {
        public static Vector2 Normalized(this Vector2 source)
            => Vector2.Normalize(source);
        public static Vector3 Normalized(this Vector3 source)
            => Vector3.Normalize(source);
        public static Vector4 Normalized(this Vector4 source)
            => Vector4.Normalize(source);

        public static float Get(this Vector3 source, int colum)
        {
            switch (colum)
            {
                case 0:
                    return source.X;
                case 1:
                    return source.Y;
                case 2:
                    return source.Z;
                default:
                    throw new IndexOutOfRangeException("Vector3 only has 3 values");
            }
        }

        public static Vector3 ProjectOnPlane(this Vector3 source, Plane plane)
        {
            return source - plane.Normal * (Vector3.Dot(source, plane.Normal) - plane.D);
        }
        public static Vector3 ProjectOnPlane(this Vector3 source, Vector3 plane)
        {
            return source - plane * Vector3.Dot(source, plane);
        }
    }
}
