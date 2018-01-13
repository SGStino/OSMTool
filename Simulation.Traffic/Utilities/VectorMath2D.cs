using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Utilities
{
    public static class VectorMath2D
    {
        public static void IntersectsLineLine(Vector2 a, Vector2 dA, Vector2 b, Vector2 dB, out float distA, out float distB)
        {
            distB = (-dA.y * (a.x - b.x) + dA.x * (a.y - b.y)) / (-dB.x * dA.y + dA.x * dB.y);
            distA = (dB.x * (a.y - b.y) - dB.y * (a.x - b.x)) / (-dB.x * dA.y + dA.x * dB.y);
        }


        public static bool IntersectsLineLine(Vector2 a, Vector2 dA, float widthA, Vector2 b, Vector2 dB, float widthB, out float distA, out float distB)
        {
            var tA = new Vector2(-dA.y, dA.x).normalized;
            var tB = new Vector2(-dB.y, dB.x).normalized;

            var dotA = Vector2.Dot(tA, dB);
            var dotB = Vector2.Dot(tB, dA);

            var dot = Vector2.Dot(dA, dB);

            if(dot > 0.9999f || dot < -0.9999f || Mathf.Abs(dotA) < 0.0001f)
            {
                distA = float.PositiveInfinity;
                distB = float.PositiveInfinity;
                return false;
            }

         

            var signA = Mathf.Sign(dotA);
            var signB = Mathf.Sign(dotB);

            tA *= signA;
            tB *= signB;

            IntersectsLineLine(a + tA * widthA * 0.5f, dA, b + tB * widthB * 0.5f, dB, out distB, out distA);
            return true;
        }



        public static bool IntersectsLineCircle(Vector2 a, Vector2 dA, Vector2 c, float radius, out float nearDistance, out float farDistance)
        {
            return IntersectsLineEllipse(a, dA, c, radius, radius, out nearDistance, out farDistance);
        }

        public static bool IntersectsLineEllipse(Vector2 a, Vector2 dA, Vector2 c, float radiusX, float radiusY, out float nearDistance, out float farDistance)
        {
            //DebugUtils.DrawEllipse(c, radiusX, radiusY, Color.yellow);
            a -= c;
            a.x /= radiusX;
            a.y /= radiusY;

            dA.x /= radiusX;
            dA.y /= radiusY;

            var A = Vector3.Dot(dA, dA);
            var B = 2 * Vector3.Dot(dA, a);
            var C = Vector3.Dot(a, a) - 1;

            var det = B * B - 4 * A * C;
            if (A <= 0.000001 || det < 0)
            {
                farDistance = nearDistance = float.NaN;
                return false;
            }
            else if (det == 0)
            {
                farDistance = nearDistance = -B / (2 * A);
                return true;
            }
            else
            {
                var sqrt = Mathf.Sqrt(det);
                var A2 = 2 * A;
                var distanceA = (-B - sqrt) / A2;
                var distanceB = (-B + sqrt) / A2;
                nearDistance = distanceA;//Mathf.Min(distanceA, distanceB);
                farDistance = distanceB;//Mathf.Max(distanceA, distanceB);
                return true;
            }
        }
    }
}
