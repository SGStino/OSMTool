using System;
using System.Numerics;

namespace Simulation.Traffic
{
    public static class BiarcInterpolation
    {
        private const float Epsilon = 0.0001f;
        //http://www.ryanjuckett.com/programming/biarc-interpolation/


        //public static void Biarc(SegmentNodeConnection start, SegmentNodeConnection end, out Arc arc1, out Arc arc2)
        //{
        //    var t1 = start.Offset.Value.Tangent;
        //    var t2 = end.Offset.Value.Tangent;

        //    var p1 = start.Node.Position.Value;
        //    var p2 = end.Node.Position.Value;
        //    Biarc(t2d(p1), t2d(t1).Normalized(), t2d(p2), t2d(-t2).Normalized(), out arc1, out arc2);
        //}

        private static Vector2 t2d(Vector3 position)
        {
            return new Vector2(position.X, position.Z);
        }

        public static void Biarc(Vector2 p1, Vector2 t1, Vector2 p2, Vector2 t2, out Arc arc1, out Arc arc2)
        {
            var v = p2 - p1;
            var vMagSqr = v.LengthSquared();

            var vDotT1 = Vector2.Dot(v, t1);

            var t = t1 + t2;
            var tMagSqr = t.LengthSquared();

            var equalTangents = MathF.Abs(tMagSqr - 4) < Epsilon;

            var perpT1 = MathF.Abs(vDotT1) < Epsilon;

            if (equalTangents && perpT1)
            {
                var angle = MathF.Atan2(v.Y, v.X);
                var center1 = p1 + v * 0.25f;
                var center2 = p1 + v * 0.75f;
                var radius = MathF.Sqrt(vMagSqr) * 0.25f;
                var cross = v.X * t1.Y - v.Y * t1.X;

                arc1 = new Arc(center1, radius, angle - MathF.PI, angle, cross < 0);
                arc2 = new Arc(center2, radius, angle - MathF.PI, angle, cross > 0);
            }
            else
            {
                float d;
                var vDotT = Vector2.Dot(v, t);

                if (equalTangents)
                {
                    d = vMagSqr / (4 * vDotT1);
                }
                else
                {
                    var denominator = 2 - 2 * Vector2.Dot(t1, t2);
                    var discriminant = vDotT * vDotT + denominator * vMagSqr;
                    d = (MathF.Sqrt(discriminant) - vDotT) / denominator;
                }

                var joint = (t1 - t2) * d;
                joint = joint + p1 + p2;
                joint = joint * 0.5f;


                arc1 = createArc(p1, t1, joint, false, false);
                arc2 = createArc(p2, t2, joint, true, true);
            }
        }

        private static Arc createArc(Vector2 p1, Vector2 t1, Vector2 p2, bool fromP1, bool reverse)
        {
            var chord = p2 - p1;
            var n1 = new Vector2(-t1.Y, t1.X);
            var chordDotN1 = Vector2.Dot(chord, n1);

            if (MathF.Abs(chordDotN1) < Epsilon)
            {
                return new Arc(p1, 0, 0, 0, false);
            }
            else
            {
                var radius = chord.LengthSquared() / (2 * chordDotN1);
                var center = p1 + n1 * radius;
                var p1Offset = p1 - center;
                var p2Offset = p2 - center;

                var p1Ang1 = MathF.Atan2(p1Offset.Y, p1Offset.X);
                var p2Ang1 = MathF.Atan2(p2Offset.Y, p2Offset.X);

                var cross = p1Offset.X * t1.Y - p1Offset.Y * t1.X;

                if (reverse)
                {
                    if (p1Ang1 < p2Ang1)
                        p1Ang1 += 2 * MathF.PI;

                    if (cross > 0)
                        return new Arc(center, MathF.Abs(radius), p2Ang1, p1Ang1, fromP1);
                    else
                        return new Arc(center, MathF.Abs(radius), p2Ang1, p1Ang1, !fromP1);
                }
                else
                {
                    if (p2Ang1 < p1Ang1)
                        p2Ang1 += 2 * MathF.PI;

                    if (cross > 0)
                        return new Arc(center, MathF.Abs(radius), p1Ang1, p2Ang1, !fromP1);
                    else
                        return new Arc(center, MathF.Abs(radius), p1Ang1, p2Ang1, fromP1);
                }
            }
        }

    }
    public struct Arc
    {
        public readonly Vector2 center;
        public readonly float radius;

        public bool IsClockwise()
        {
            return reverse;
        }

        public readonly float startAngle;
        public readonly float endAngle;
        public readonly float angle;
        public readonly float arcLength;
        public readonly bool reverse;
        public Arc(Vector2 center, float radius, float startAngle, float endAngle, bool reverse)
        {
            this.center = center;
            this.radius = radius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;

            if (endAngle < startAngle)
                endAngle += 2 * MathF.PI;

            float angleDistance = endAngle - startAngle;
            if (reverse || angleDistance == 0)
                angle = angleDistance;
            else
                angle = -(MathF.PI * 2 - angleDistance);

            this.arcLength = MathF.Abs(angle) * radius;
            this.reverse = reverse;

        }




        internal Vector2 InterpolateTangent(float p)
        {
            var a = InterpolateAngle(p);
            var cos = MathF.Cos(a);
            var sin = MathF.Sin(a);



            if (reverse)
                return new Vector2(cos, sin);
            return new Vector2(-cos, -sin);
        }

        public float InterpolateAngle(float p)
        {
            return startAngle + angle * p;
        }
        public Vector2 Interpolate(float p, float offset = 0)
        {
            var a = InterpolateAngle(p);

            var r = radius + (IsClockwise() ? offset : -offset);
            var cos = MathF.Cos(a) * r;
            var sin = MathF.Sin(a) * r;



            return center + new Vector2(cos, sin);
        }


        public Vector2 GetClosestPoint(Vector2 position)
        {
            float p;
            GetClosestPoint(position, false, 0, out p);
            return Interpolate(p);
        }

        public bool GetClosestPoint(Vector2 position, bool clip, float offset, out float snapValue)
        {
            var r = position - center;
            var n =  Vector2.Normalize( r);

            float posAngle = MathF.Atan2(n.Y, n.X);

            if (reverse)
                offset = -offset;



            var min = startAngle * MathF.Rad2Deg;
            var max = (startAngle + this.angle) * MathF.Rad2Deg;
            var a = posAngle * MathF.Rad2Deg;

            var isInArc = Angles.IsAngleBetweenDegrees(a, min, max);
            if (!isInArc)
            {
                if (clip)
                {
                    snapValue = float.NaN;
                    return false;
                }
                else
                {
                    //TODO: solve this without interpolate
                    var startPoint = Interpolate(0, offset);
                    var endPoint = Interpolate(1, offset);

                    if ((startPoint - position).LengthSquared() < (endPoint - position).LengthSquared())
                        snapValue = 0;
                    else
                        snapValue = 1;
                    return true;
                }
            }

            snapValue = (a - min) / (max - min);
            return true;
        }

        public bool IsGreatArc()
        {
            return Math.Abs(angle) > Math.PI;
        }
    }
}
