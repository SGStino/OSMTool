using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Utilities
{
    public static class VectorMath3D
    {
        public static Vector2 GetXZ(this Vector3 input) => new Vector2(input.x, input.z);
        public static Vector3 GetTranslate(this Matrix4x4 input) => input.MultiplyPoint3x4(Vector3.zero);
        public static Vector3 GetForward(this Matrix4x4 input) => input.MultiplyVector(Vector3.forward);
        public static Vector3 GetRight(this Matrix4x4 input) => input.MultiplyVector(Vector3.right);
        public static Vector3 GetUp(this Matrix4x4 input) => input.MultiplyVector(Vector3.up);


        public static float GetAngle(Vector3 start, Vector3 end, Vector3 normal)
        {
            var dot = Vector3.Dot(start.normalized, end.normalized);
            dot = Mathf.Clamp(dot, -1, 1);// clean up edge cases with floating point precision
            var angle = Mathf.Acos(dot);
            var cross = Vector3.Cross(start, end);


            var d = Vector3.Dot(normal, cross);
            if (d < 0)
                return -angle;
            return angle;
        }
        public static Vector3 GetPointOnSegment(Vector3 start, Vector3 end, Vector3 point, out float distance)
        {
            var dir = end - start;
            var len = dir.magnitude;
            dir /= len;
            return GetPointOnSegment(start, dir, len, point, out distance);
        }

        public static Vector3 GetPointOnSegment(Vector3 start, Vector3 dir, float length, Vector3 point, out float distance)
        {
            var offset = point - start;
            var dot = Vector3.Dot(offset, dir) / length;

            distance = Mathf.Clamp01(dot) * length;
            return distance * dir + start;
        }

        public static Vector3 GetPointOnCircle(Vector3 normal, float radius, Vector3 vector)
        {
            var dir = Vector3.ProjectOnPlane(vector, normal);

            var len = dir.magnitude;
            dir /= len;

            return dir * radius;
        }

        public static Ray Intersect(Plane plane1, Plane plane2)
        {
            var dir = Vector3.Cross(plane1.normal, plane2.normal);

            var p1 = plane2.ClosestPointOnPlane(plane1.normal * plane1.distance);
            return new Ray(p1, dir);
        }

        public static bool IntersectCircle(Plane plane, Vector3 center, Vector3 normal, Vector3 forward, out float[] angles)
        {
            var circlePlane = new Plane(normal, center);

            var ray = Intersect(plane, circlePlane);


            var matrix = Matrix4x4.LookAt(center, center + forward, normal).inverse;

            var o = matrix.MultiplyPoint3x4(ray.origin);
            var d = matrix.MultiplyVector(ray.direction);

            throw new NotImplementedException("Intersect ray-circle in 3D without y => 2D (XZ)");
        }
    }
}
