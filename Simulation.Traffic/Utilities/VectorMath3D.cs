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

        public static float GetAngle(Vector3 start, Vector3 end, Vector3 normal)
        {
            var dot = Vector3.Dot(start.normalized, end.normalized);
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
    }
}
