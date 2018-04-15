using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Utilities
{
    public static class VectorMath3D
    {
        public static Vector2 GetXZ(this Vector3 input) => new Vector2(input.X, input.Z);
        public static Vector3 GetTranslate(this Matrix4x4 input) => input.MultiplyPoint3x4(Vector3.Zero);
        public static Vector3 GetForward(this Matrix4x4 input) => input.MultiplyVector(Directions3.Forward);
        public static Vector3 GetRight(this Matrix4x4 input) => input.MultiplyVector(Directions3.Right);
        public static Vector3 GetUp(this Matrix4x4 input) => input.MultiplyVector(Directions3.Up);


        public static float GetAngle(Vector3 start, Vector3 end, Vector3 normal)
        {
            var dot = Vector3.Dot(Vector3.Normalize(start), Vector3.Normalize(end));
            dot = MathF.Clamp(dot, -1, 1);// clean up edge cases with floating point precision
            var angle = MathF.Acos(dot);
            var cross = Vector3.Cross(start, end);


            var d = Vector3.Dot(normal, cross);
            if (d < 0)
                return -angle;
            return angle;
        }
        public static void GetPointOnSegment(Vector3 start, Vector3 end, Vector3 point, out float distance)
        {
            var dir = end - start;
            var len = dir.Length();
            dir /= len;
            GetPointOnSegment(start, dir, len, point, out distance);
        }

        public static void GetPointOnSegment(Vector3 start, Vector3 dir, float length, Vector3 point, out float distance)
        {
            var offset = point - start;
            var dot = Vector3.Dot(offset, dir) / length;

            distance = MathF.Clamp01(dot) * length;
            ///*return*/ distance * dir + start;
        }
        internal static void NotZero(this Vector3 vector)
        {
            if (vector.LengthSquared() < 0.0001f)
                throw new InvalidOperationException("Vector has Zero Length");

        }
        internal static void NotNaN(this Vector3 result)
        {
            if (float.IsNaN(result.X))
                throw new InvalidOperationException("Vector X contains NaN");
            if (float.IsNaN(result.Y))
                throw new InvalidOperationException("Vector Y contains NaN");
            if (float.IsNaN(result.Z))
                throw new InvalidOperationException("Vector Z contains NaN");
        }
        internal static void NotNaN(this Matrix4x4 result)
        {
            if (float.IsNaN(result.M11))
                throw new InvalidOperationException("Matrix M11 contains NaN");
            if (float.IsNaN(result.M12))
                throw new InvalidOperationException("Matrix M12 contains NaN");
            if (float.IsNaN(result.M13))
                throw new InvalidOperationException("Matrix M13 contains NaN");
            if (float.IsNaN(result.M14))
                throw new InvalidOperationException("Matrix M14 contains NaN");

            if (float.IsNaN(result.M21))
                throw new InvalidOperationException("Matrix M21 contains NaN");
            if (float.IsNaN(result.M22))
                throw new InvalidOperationException("Matrix M22 contains NaN");
            if (float.IsNaN(result.M23))
                throw new InvalidOperationException("Matrix M23 contains NaN");
            if (float.IsNaN(result.M24))
                throw new InvalidOperationException("Matrix M24 contains NaN");


            if (float.IsNaN(result.M31))
                throw new InvalidOperationException("Matrix M31 contains NaN");
            if (float.IsNaN(result.M32))
                throw new InvalidOperationException("Matrix M32 contains NaN");
            if (float.IsNaN(result.M33))
                throw new InvalidOperationException("Matrix M33 contains NaN");
            if (float.IsNaN(result.M34))
                throw new InvalidOperationException("Matrix M34 contains NaN");

            if (float.IsNaN(result.M41))
                throw new InvalidOperationException("Matrix M41 contains NaN");
            if (float.IsNaN(result.M42))
                throw new InvalidOperationException("Matrix M42 contains NaN");
            if (float.IsNaN(result.M43))
                throw new InvalidOperationException("Matrix M43 contains NaN");
            if (float.IsNaN(result.M44))
                throw new InvalidOperationException("Matrix M44 contains NaN");
        }

        public static Vector3 GetPointOnCircle(Vector3 normal, float radius, Vector3 vector)
        {
            var dir = vector.ProjectOnPlane(normal);

            var len = dir.Length();
            dir /= len;

            return dir * radius;
        }

        public static bool Intersect(Plane plane1, Plane plane2, out Ray intersection)
        {
            var dir = Vector3.Cross(plane1.Normal, plane2.Normal);
            var det = dir.LengthSquared();

            if (MathF.Abs(det) >= 0.000001f)
            {
                var p = (Vector3.Cross(dir, plane2.Normal) * plane1.D + Vector3.Cross(plane1.Normal, dir) * plane2.D) / det;

                intersection = new Ray(p, dir);
                return true;
            }

            intersection = default(Ray);
            return false;
        }

        public static bool IntersectCircle(Plane plane, Vector3 center, Vector3 normal, Vector3 forward, float radius, out float angle1, out float angle2)
        {
            var circlePlane = new Plane(normal, Vector3.Dot(normal, center));

            if (Intersect(plane, circlePlane, out var ray))
            {

                var matrix = Matrix4x4.CreateWorld(center, forward, normal);

                if (Matrix4x4.Invert(matrix, out matrix))
                {

                    var o = matrix.MultiplyPoint3x4(ray.Origin);
                    var d = matrix.MultiplyVector(ray.Direction);

                    var f = matrix.MultiplyVector(forward);

                    float near, far;
                    if (VectorMath2D.IntersectsLineCircle(o.GetXZ(), d.GetXZ(), Vector2.Zero, radius, out near, out far))
                    {
                        if (near == far)
                        {
                            angle1 = angle2 = VectorMath3D.GetAngle(o + d * near, Directions3.Forward, Directions3.Up);
                            return true;
                        }
                        // TODO: Solve with VectorMath2D for performance gain
                        angle1 = VectorMath2D.GetAngle(Directions2.Up, o.GetXZ() + d.GetXZ() * near);
                        angle2 = VectorMath2D.GetAngle(Directions2.Up, o.GetXZ() + d.GetXZ() * far);
                        return true;
                    }
                }
                angle1 = angle2 = float.NaN;
                return false;
            }
            angle1 = angle2 = float.NaN;
            return false;
        }
    }
}
