
using Simulation.Data.Primitives;
using Simulation.Traffic.Utilities;
using System;
using System.Numerics;

namespace Simulation.Traffic.Lofts
{
    public class BiArcLoftPath : ILoftPath
    {
        private ILoftPath arc1;
        private ILoftPath arc2;

        public ILoftPath Arc1 { get { return arc1; } }
        public ILoftPath Arc2 { get { return arc2; } }
        private Vector3 t3d(Vector2 input)
        {
            return new Vector3(input.X, 0, input.Y);
        }
        public BiArcLoftPath(Vector3 start, Vector3 tangentStart, Vector3 end, Vector3 tangentEnd)
        {
#if DEBUG
            if (float.IsNaN(start.X)) throw new InvalidOperationException("start.X is not a number");
            if (float.IsNaN(start.Y)) throw new InvalidOperationException("start.Y is not a number");
            if (float.IsNaN(start.Z)) throw new InvalidOperationException("start.Z is not a number");
            if (float.IsNaN(end.X)) throw new InvalidOperationException("end.X is not a number");
            if (float.IsNaN(end.Y)) throw new InvalidOperationException("end.Y is not a number");
            if (float.IsNaN(end.Z)) throw new InvalidOperationException("end.Z is not a number");
            if (float.IsNaN(tangentStart.X)) throw new InvalidOperationException("tangentStart.X is not a number");
            if (float.IsNaN(tangentStart.Y)) throw new InvalidOperationException("tangentStart.Y is not a number");
            if (float.IsNaN(tangentStart.Z)) throw new InvalidOperationException("tangentStart.Z is not a number");
            if (float.IsNaN(tangentEnd.X)) throw new InvalidOperationException("tangentEnd.X is not a number");
            if (float.IsNaN(tangentEnd.Y)) throw new InvalidOperationException("tangentEnd.Y is not a number");
            if (float.IsNaN(tangentEnd.Z)) throw new InvalidOperationException("tangentEnd.Z is not a number");

            var distSqr = (start - end).LengthSquared();
            if (distSqr < 0.001)
                throw new InvalidOperationException("Zero Length");
#endif
            tangentStart = Vector3.Normalize(tangentStart);             
            tangentEnd = Vector3.Normalize(tangentEnd);

            var pointStart = start + tangentStart;
            var pointEnd = end + tangentEnd;

            var c1 = (start + end) / 2;
            var c2 = (pointStart + pointEnd) / 2;

            var n1 = Vector3.Normalize( end - start);
            var n2 = Vector3.Normalize(pointEnd - pointStart); 

            //Debug.DrawLine(pointStart, pointEnd);

            var d1 = Vector3.Dot(n1, c1);
            var d2 = Vector3.Dot(n2, c2);

            //DebugUtils.DrawPlane(c1, n1, .01f, Color.red);
            //DebugUtils.DrawPlane(c2, n2, .01f, Color.red);

            //Debug.DrawLine(Vector3.Zero, n1 * d1);
            //Debug.DrawLine(Vector3.Zero, n2 * d2);

            Ray r12;
            if (intersect(n1, d1, n2, d2, out r12))
            {
                //Debug.DrawLine(r12.origin, r12.GetPoint(10), Color.blue);

                var r = Vector3.Dot(start - r12.Origin, r12.Direction);

                var center1 = r12.GetPoint(r);

                //Debug.DrawLine(center1, start, Color.Yellow);
                //Debug.DrawLine(center1, end, Color.Yellow);
#if DEBUG
                if (float.IsNaN(center1.X)) throw new InvalidOperationException("center1.X is not a number");
                if (float.IsNaN(center1.Y)) throw new InvalidOperationException("center1.Y is not a number");
                if (float.IsNaN(center1.Z)) throw new InvalidOperationException("center1.Z is not a number");
#endif

                var rH = (center1 - start).Length();
                var rC = (center1 - c1).Length();
#if DEBUG
                if (float.IsNaN(rH)) throw new InvalidOperationException("rH is not a number");
                if (float.IsNaN(rC)) throw new InvalidOperationException("rC is not a number");
#endif

                Vector3 pointH1, pointH2;
                if (MathF.Abs(rC) > 0.00001f)
                {
                    var lCH = (c1 - center1) / rC * rH;
                    //DebugUtils.DrawCircle(center1, rH, r12.direction, Color.Yellow);
#if DEBUG
                    if (float.IsNaN(lCH.X)) throw new InvalidOperationException("lCH.X is not a number");
                    if (float.IsNaN(lCH.Y)) throw new InvalidOperationException("lCH.Y is not a number");
                    if (float.IsNaN(lCH.Z)) throw new InvalidOperationException("lCH.Z is not a number");
#endif
                    pointH2 = center1 + lCH;
                    pointH1 = center1 - lCH;
                }
                else
                {
                    pointH1 = pointH2 = center1 - tangentStart * (start - end).Length() / 2;
                }
                //Debug.DrawLine(pointH1, pointH2, Color.green);

                //arc1 = getArc(start, tangentStart, pointH1, true);
                //arc2 = getArc(end, tangentEnd, pointH1, false);

                var arc1start = getArc(start, tangentStart, pointH1, true);
                var arc2start = getArc(start, tangentStart, pointH2, true);

                var arc1end = getArc(end, tangentEnd, pointH1, false);
                var arc2end = getArc(end, tangentEnd, pointH2, false);

                if (arc1start.Length + arc1end.Length < arc2start.Length + arc2end.Length)
                {
                    arc1 = arc1start;
                    arc2 = arc1end;
                }
                else
                {
                    arc1 = arc2start;
                    arc2 = arc2end;
                }
                return;
            }

            // TODO: possible anomaly: both tangents are identical: perpendicular planes are parallel
            var center = (start + end) / 2;
            arc1 = getArc(start, tangentStart, center, true);
            arc2 = getArc(end, tangentEnd, center, false);
        }

        private string asBin(Vector3 start)
        {
            return asBin(start.X) + " " + asBin(start.Y) + " " + asBin(start.Z);
        }

        private string asBin(float z)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(z));
        }

        private ILoftPath getArc(Vector3 point, Vector3 tangent, Vector3 pointH, bool reverse)
        {
#if DEBUG
            if (float.IsNaN(point.X)) throw new InvalidOperationException("point.X is not a number");
            if (float.IsNaN(point.Y)) throw new InvalidOperationException("point.Y is not a number");
            if (float.IsNaN(point.Z)) throw new InvalidOperationException("point.Z is not a number");
            if (float.IsNaN(tangent.X)) throw new InvalidOperationException("tangent.X is not a number");
            if (float.IsNaN(tangent.Y)) throw new InvalidOperationException("tangent.Y is not a number");
            if (float.IsNaN(tangent.Z)) throw new InvalidOperationException("tangent.Z is not a number");
            if (float.IsNaN(pointH.X)) throw new InvalidOperationException("pointH.X is not a number");
            if (float.IsNaN(pointH.Y)) throw new InvalidOperationException("pointH.Y is not a number");
            if (float.IsNaN(pointH.Z)) throw new InvalidOperationException("pointH.Z is not a number");
#endif

            //Debug.DrawLine(point, point + tangent * 10);
            var lHP = pointH - point;

            var pHSC = (pointH + point) / 2;

            var pHSn = Vector3.Normalize(lHP);

            var pHSd = Vector3.Dot(pHSn, pHSC);

            var pSn = tangent;
            var pSd = Vector3.Dot(pSn, point);

            //DebugUtils.DrawPlane(pHSC, pHSn, 0.05f, Color.cyan);
            //DebugUtils.DrawPlane(point, pSn, 0.05f, Color.cyan);

            Ray rS;
            if (intersect(pSn, pSd, pHSn, pHSd, out rS))
            {
                var center = rS.GetPoint(Vector3.Dot(rS.Direction, point - rS.Origin));
                //Debug.DrawLine(rS.origin, center, Color.blue);
                //DebugUtils.DrawCircle(center, (center - point).Length(), rS.direction, Color.cyan);


                //Debug.DrawLine(point, centerStart, Color.magenta);
                //Debug.DrawLine(pointH, centerStart, Color.magenta);


                // TODO: it isn't always shortest angle

                var side = Vector3.Cross(tangent, Directions3.Up);
                var dot = Vector3.Dot(side, point - center);

                //if (MathF.Abs(dot) < 0.0001f)
                //{
                //    side = Vector3.Cross(tangent, Directions3.Right);
                //    dot = Vector3.Dot(side, point - center);
                //}

                var ccw = dot > 0;
                return reverse ? ArcLoftPath.Create(center, point, pointH, ccw) : ArcLoftPath.Create(center, pointH, point, ccw);
            }
            return reverse ? new LinearPath(point, pointH) : new LinearPath(pointH, point);
        }

        private Vector3 getNormal(Vector3 pointH1, Vector3 start, Vector3 pointStart)
        {
            var v1 = pointH1 - start;
            var v2 = pointStart - start;
            return Vector3.Cross(v1, v2);
        }

        private bool intersect(Vector3 p1n, float p1d, Vector3 p2n, float p2d, out Ray ray)
        {
            var p3n = Vector3.Cross(p1n, p2n);
            var det = p3n.LengthSquared();
            if (det >= 0.000001)
            {
                var p3 = ((Vector3.Cross(p2n, p3n) * p1d) + (Vector3.Cross(p3n, p1n) * p2d)) / det;
                ray = new Ray(p3, p3n);
                return true;
            }

            ray = default(Ray);
            return false;
        }
        /*
        public BiArcLoftPath(Vector2 start, Vector2 tangentStart, Vector2 end, Vector2 tangentEnd)
        {
            //ref: https://www.geogebra.org/m/qFlEelg3

            tangentStart.Normalize();
            tangentEnd.Normalize();

            var biTangentStart = makeBiTangent(tangentStart);
            var biTangentEnd = makeBiTangent(tangentEnd);

            var pointStart = start + tangentStart;
            var pointEnd = end + tangentEnd;

            var c1 = (start + end) / 2;
            var c2 = (pointStart + pointEnd) / 2;

            var l1 = end - start;
            var l2 = pointEnd - pointStart;


            var t1 = makeBiTangent(l1);
            var t2 = makeBiTangent(l2);


            var dotStart = Vector2.Dot(t1, tangentStart);
            var dotEnd = Vector2.Dot(t1, tangentEnd);


            if (MathF.Abs(dotStart) < 0.1)
            {
                // straight start
                if (MathF.Abs(dotEnd) < 0.1)
                {
                    // straight end
                    arc1 = new LinearPath(t3d(start), t3d(c1));
                    arc2 = new LinearPath(t3d(c1), t3d(end));
                    return;
                }
            }


            //Debug.DrawLine(t3d(end), t3d(start), Color.black);
            //Debug.DrawLine(t3d(start), t3d(pointStart), Color.red);
            //Debug.DrawLine(t3d(end), t3d(pointEnd), Color.blue);

            //Debug.DrawLine(t3d(pointEnd), t3d(pointStart), Color.black);

            //Debug.DrawRay(t3d(c1), t3d(t1.Normalized() * 2), Color.gray);
            //Debug.DrawRay(t3d(c2), t3d(t2.Normalized() * 2), Color.green);

            float u, v;
            intersect(c1, t1, c2, t2, out u, out v);

            Vector2 pointH1, pointH2;
            if (float.IsInfinity(u) || float.IsNaN(v))
            {
                // anomaly 1: the first circle is of infinite size
                // just put the point in the middle
                pointH1 = pointH2 = c1;
            }
            else
            {
                var center1 = c1 + t1 * u;
                var rc1 = (center1 - start).Length();
                // STEP 3: create point H somewhere on the first Circle (we take halfway between start and end)

                var dir = Vector2.Dot(t2, tangentStart)
                    + Vector2.Dot(t2, tangentEnd);

                var distCenter2pointH = (rc1 / t1.Length());

                pointH1 = center1 + t1 * distCenter2pointH;
                pointH2 = center1 - t1 * distCenter2pointH;

                //Debug.DrawRay(t3d(c1), t3d(t1.Normalized() * 2 * MathF.Sign(u)), Color.red);

                //Debug.DrawLine(t3d(center1), t3d(start), Color.magenta);
                //Debug.DrawLine(t3d(center1), t3d(end), Color.cyan);

                //Debug.DrawLine(t3d(pointH1), t3d(start), Color.Yellow);
                //Debug.DrawLine(t3d(pointH1), t3d(end), Color.Yellow);
                //Debug.DrawLine(t3d(pointH2), t3d(start), Color.Yellow);
                //Debug.DrawLine(t3d(pointH2), t3d(end), Color.Yellow);

                //DebugUtils.DrawCircle(t3d(center1), (center1 - start).Length(), Directions3.Up, Color.white);

            }
            var pointH = pointH1;

            var arc1A = getSegment(start, biTangentStart, pointH1, true);
            var arc1B = getSegment(start, biTangentStart, pointH2, true);


            var arc2A = getSegment(end, biTangentEnd, pointH1, false);
            var arc2B = getSegment(end, biTangentEnd, pointH2, false);

            var aLength = arc1A.Length + arc2A.Length;
            var bLength = arc1B.Length + arc2B.Length;

            arc1 = aLength > bLength ? arc1B : arc1A;
            arc2 = aLength > bLength ? arc2B : arc2A;
            if (Length > 10000)
            {
                Debug.LogWarning("Large biarc!");
            }
        }

        private ILoftPath getSegment(Vector2 refPoint, Vector2 tangent, Vector2 pointH, bool othersegment)
        {
            float u; float v;
            var centerPointHToRefPoint = (pointH + refPoint) / 2;
            var tangentPointHToRefPoint = makeBiTangent(refPoint - pointH);

            if (intersect(refPoint, tangent, centerPointHToRefPoint, tangentPointHToRefPoint, out u, out v))
            {
                var center = refPoint + tangent * u;
                var arcStartVector = (othersegment ? refPoint : pointH) - center;
                var isClockwise = u > 0;
                var angle = getAngle(arcStartVector, (othersegment ? pointH : refPoint) - center, isClockwise);

                if (MathF.Abs(angle) < 0.001)
                    return getLinear(refPoint, pointH, othersegment);
                else
                    return new ArcLoftPath(t3d(arcStartVector), -angle, t3d(center), Directions3.Up);
            }
            else
                return getLinear(refPoint, pointH, false);
        }

        private ILoftPath getLinear(Vector2 refPoint, Vector2 pointH, bool reverse)
        {
            if (!reverse)
                return new LinearPath(t3d(pointH), t3d(refPoint));
            else
                return new LinearPath(t3d(refPoint), t3d(pointH));
        }

        private float getAngle(Vector2 v1, Vector2 v2, bool clockWise)
        {
            var dot = v1.X * v2.X + v1.Y * v2.Y;
            var det = v1.X * v2.Y - v1.Y * v2.X;
            var a = MathF.Atan2(det, dot);
            if (a < 0) a += MathF.PI * 2;
            if (!clockWise)
                a = -(MathF.PI * 2 - a);
            return a;
        }



        private bool intersect(Vector2 p1, Vector2 d1, Vector2 p2, Vector2 d2, out float u, out float v)
        {
            u = (p1.Y * d2.X + d2.Y * p2.X - p2.Y * d2.X - d2.Y * p1.X) / (d1.X * d2.Y - d1.Y * d2.X);
            v = (p1.X + d1.X * u - p2.X) / d2.X;
            return !float.IsInfinity(u);
        }

        private Vector2 makeBiTangent(Vector2 t)
        {
            return new Vector2(t.Y, -t.X);
        }
        private Vector3 makeBiTangent(Vector3 t)
        {
            return Vector3.Cross(Directions3.Up, t);
        }
        */
        public float Length { get { return arc1.Length + arc2.Length; } }

        public Matrix4x4 GetTransform(float distance)
        {
            if (distance > arc1.Length)
            {
                distance -= arc1.Length;
                return arc2.GetTransform(distance);
            }
            return arc1.GetTransform(distance);
        }

        public void SnapTo(Vector3 to, out float distance)
        {

            float d1, d2;



            arc1.SnapTo(to, out d1);
            arc2.SnapTo(to, out d2);

            var p1 = arc1.GetTransform(d1).GetTranslate();
            var p2 = arc2.GetTransform(d2).GetTranslate();

            //Debug.DrawLine(p1, to, Color.blue);
            //Debug.DrawLine(p2, to, Color.red);

            var l1 = (p1 - to).Length();
            var l2 = (p2 - to).Length();

            if (l1 < l2)
            {
                distance = d1;
            }
            else
            {
                distance = d2 + arc1.Length;
            }
        }

        public bool Intersects(Plane plane, out float[] loftDistances)
        {
            loftDistances = new float[0];
            bool intersect = false;
            if (arc1.Intersects(plane, out float[] distancesA))
            {
                var start = loftDistances.Length;
                Array.Resize(ref loftDistances, start + distancesA.Length);
                for (int i = 0; i < distancesA.Length; i++)
                    loftDistances[start + i] = distancesA[i];
                intersect = true;
            }
            if (arc2.Intersects(plane, out float[] distancesB))
            {
                var start = loftDistances.Length;
                Array.Resize(ref loftDistances, start + distancesB.Length);
                for (int i = 0; i < distancesB.Length; i++)
                    loftDistances[start + i] = distancesB[i] + arc1.Length;
                intersect = true;
            }
            return intersect;
        }

        public Rectangle GetBounds(float width)
        {
            var b1 = arc1.GetBounds(width);
            var b2 = arc2.GetBounds(width);

            var minX = MathF.Min(b1.Min.X, b2.Min.X);
            var maxX = MathF.Max(b1.Max.X, b2.Max.X);
            var minY = MathF.Min(b1.Min.Y, b2.Min.Y);
            var maxY = MathF.Max(b1.Max.Y, b2.Max.Y);
            return Rectangle.MinMaxRectangle(minX, minY, maxX, maxY);
        }
    }
}
