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
        public static void GetPointOnSegment(Vector3 start, Vector3 end, Vector3 point, out float distance)
        {
            var dir = end - start;
            var len = dir.magnitude;
            dir /= len;
              GetPointOnSegment(start, dir, len, point, out distance);
        }

        public static void GetPointOnSegment(Vector3 start, Vector3 dir, float length, Vector3 point, out float distance)
        {
            var offset = point - start;
            var dot = Vector3.Dot(offset, dir) / length;

            distance = Mathf.Clamp01(dot) * length;
            ///*return*/ distance * dir + start;
        }

        public static Vector3 GetPointOnCircle(Vector3 normal, float radius, Vector3 vector)
        {
            var dir = Vector3.ProjectOnPlane(vector, normal);

            var len = dir.magnitude;
            dir /= len;

            return dir * radius;
        }

        public static bool Intersect(Plane plane1, Plane plane2, out Ray intersection)
        {
            var dir = Vector3.Cross(plane1.normal, plane2.normal);
            var det = dir.sqrMagnitude;

            if (Mathf.Abs(det) >= 0.000001f)
            {
                var p = (Vector3.Cross(dir, plane2.normal) * plane1.distance + Vector3.Cross(plane1.normal, dir) * plane2.distance) / det;

                intersection = new Ray(p, dir);
                return true;
            }

            intersection = default(Ray);
            return false;
        }

        public static bool IntersectCircle(Plane plane, Vector3 center, Vector3 normal, Vector3 forward, float radius, out float angle1, out float angle2)
        {
            var circlePlane = new Plane(normal, center);

            if (Intersect(plane, circlePlane, out var ray))
            {

                var matrix = Matrix4x4.LookAt(center, center + forward, normal).inverse;

                var o = matrix.MultiplyPoint3x4(ray.origin);
                var d = matrix.MultiplyVector(ray.direction);

                var f = matrix.MultiplyVector(forward);

                float near, far;
                if (VectorMath2D.IntersectsLineCircle(o.GetXZ(), d.GetXZ(), Vector2.zero, radius, out near, out far))
                {
                    if (near == far)
                    { 
                        angle1 = angle2 = VectorMath3D.GetAngle(o + d * near, Vector3.forward, Vector3.up);
                        return true;
                    }
                    // TODO: Solve with VectorMath2D for performance gain
                    angle1 = VectorMath2D.GetAngle(Vector2.up, o.GetXZ() + d.GetXZ() * near);
                    angle2 = VectorMath2D.GetAngle(Vector3.up, o.GetXZ() + d.GetXZ() * far);
                    return true;
                }

                angle1 = angle2 = float.NaN;
                return false;
            }
            angle1 = angle2 = float.NaN;
            return false;
        }
    }
}
