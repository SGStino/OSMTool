using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Utilities
{
    public static class VectorMath2D
    {
        public static void IntersectsLineLine(Vector2 a, Vector2 dA, Vector2 b, Vector2 dB, out float distA, out float distB)
        {
            distB = (-dA.Y * (a.X - b.X) + dA.X * (a.Y - b.Y)) / (-dB.X * dA.Y + dA.X * dB.Y);
            distA = (dB.X * (a.Y - b.Y) - dB.Y * (a.X - b.X)) / (-dB.X * dA.Y + dA.X * dB.Y);
        }


        public static bool IntersectsLineLine(Vector2 a, Vector2 dA, float widthA, Vector2 b, Vector2 dB, float widthB, out float distA, out float distB)
        {
            var tA = new Vector2(-dA.Y, dA.X).Normalized();
            var tB = new Vector2(-dB.Y, dB.X).Normalized();

            var dotA = Vector2.Dot(tA, dB);
            var dotB = Vector2.Dot(tB, dA);

            var dot = Vector2.Dot(dA, dB);

            if (dot > 0.9999f || dot < -0.9999f || MathF.Abs(dotA) < 0.0001f)
            {
                distA = float.PositiveInfinity;
                distB = float.PositiveInfinity;
                return false;
            }



            var signA = MathF.Sign(dotA);
            var signB = MathF.Sign(dotB);

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
            //DebugUtils.DrawEllipse(c, radiusX, radiusY, Color.Yellow);
            a -= c;
            a.X /= radiusX;
            a.Y /= radiusY;

            dA.X /= radiusX;
            dA.Y /= radiusY;

            var A = Vector2.Dot(dA, dA);
            var B = 2 * Vector2.Dot(dA, a);
            var C = Vector2.Dot(a, a) - 1;

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
                var sqrt = MathF.Sqrt(det);
                var A2 = 2 * A;
                var distanceA = (-B - sqrt) / A2;
                var distanceB = (-B + sqrt) / A2;
                nearDistance = distanceA;//MathF.Min(distanceA, distanceB);
                farDistance = distanceB;//MathF.Max(distanceA, distanceB);
                return true;
            }
        }

        internal static float GetAngle(Vector2 start, Vector2 end)
        {
            var dot = Vector2.Dot(Vector2.Normalize(start), Vector2.Normalize(end));
            dot = MathF.Clamp(dot, -1, 1);// clean up edge cases with floating point precision
            var angle = MathF.Acos(dot);
            var d = end.X * start.Y - end.Y * start.X;
            if (d < 0)
                return -angle;
            return angle;
        }
    }
}
