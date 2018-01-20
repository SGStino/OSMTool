
using System;
using UnityEngine;

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
            return new Vector3(input.x, 0, input.y);
        }

        public BiArcLoftPath(Vector3 start, Vector3 tangentStart, Vector3 end, Vector3 tangentEnd)
        {
#if DEBUG
            if (float.IsNaN(start.x)) throw new InvalidOperationException("start.x is not a number");
            if (float.IsNaN(start.y)) throw new InvalidOperationException("start.y is not a number");
            if (float.IsNaN(start.z)) throw new InvalidOperationException("start.z is not a number");
            if (float.IsNaN(end.x)) throw new InvalidOperationException("end.x is not a number");
            if (float.IsNaN(end.y)) throw new InvalidOperationException("end.y is not a number");
            if (float.IsNaN(end.z)) throw new InvalidOperationException("end.z is not a number");
            if (float.IsNaN(tangentStart.x)) throw new InvalidOperationException("tangentStart.x is not a number");
            if (float.IsNaN(tangentStart.y)) throw new InvalidOperationException("tangentStart.y is not a number");
            if (float.IsNaN(tangentStart.z)) throw new InvalidOperationException("tangentStart.z is not a number");
            if (float.IsNaN(tangentEnd.x)) throw new InvalidOperationException("tangentEnd.x is not a number");
            if (float.IsNaN(tangentEnd.y)) throw new InvalidOperationException("tangentEnd.y is not a number");
            if (float.IsNaN(tangentEnd.z)) throw new InvalidOperationException("tangentEnd.z is not a number");
#endif

            tangentStart.Normalize();
            tangentEnd.Normalize();

            var pointStart = start + tangentStart;
            var pointEnd = end + tangentEnd;

            var c1 = (start + end) / 2;
            var c2 = (pointStart + pointEnd) / 2;

            var n1 = end - start;
            var n2 = pointEnd - pointStart;
            n1.Normalize();
            n2.Normalize();

            //Debug.DrawLine(pointStart, pointEnd);

            var d1 = Vector3.Dot(n1, c1);
            var d2 = Vector3.Dot(n2, c2);

            //DebugUtils.DrawPlane(c1, n1, .01f, Color.red);
            //DebugUtils.DrawPlane(c2, n2, .01f, Color.red);

            //Debug.DrawLine(Vector3.zero, n1 * d1);
            //Debug.DrawLine(Vector3.zero, n2 * d2);

            Ray r12;
            if (intersect(n1, d1, n2, d2, out r12))
            {
                //Debug.DrawLine(r12.origin, r12.GetPoint(10), Color.blue);

                var center1 = r12.GetPoint(Vector3.Dot(start - r12.origin, r12.direction));
                //Debug.DrawLine(center1, start, Color.yellow);
                //Debug.DrawLine(center1, end, Color.yellow);

                var rH = (center1 - start).magnitude;
                var rC = (center1 - c1).magnitude;

                var lCH = (c1 - center1) / rC * rH;
                //DebugUtils.DrawCircle(center1, rH, r12.direction, Color.yellow);

                var pointH2 = center1 + lCH;
                var pointH1 = center1 - lCH;
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
            }
            else
            {
                // TODO: possible anomaly: both tangents are identical: perpendicular planes are parallel
                var center = (start + end) / 2;
                arc1 = getArc(start, tangentStart, center, true);
                arc2 = getArc(end, tangentEnd, center, false);
            }

        }

        private ILoftPath getArc(Vector3 point, Vector3 tangent, Vector3 pointH, bool reverse)
        {
            //Debug.DrawLine(point, point + tangent * 10);
            var lHP = pointH - point;

            var pHSC = (pointH + point) / 2;

            var pHSn = lHP.normalized;

            var pHSd = Vector3.Dot(pHSn, pHSC);

            var pSn = tangent;
            var pSd = Vector3.Dot(pSn, point);

            //DebugUtils.DrawPlane(pHSC, pHSn, 0.05f, Color.cyan);
            //DebugUtils.DrawPlane(point, pSn, 0.05f, Color.cyan);

            Ray rS;
            if (intersect(pSn, pSd, pHSn, pHSd, out rS))
            {
                var center = rS.GetPoint(Vector3.Dot(rS.direction, point - rS.origin));
                //Debug.DrawLine(rS.origin, center, Color.blue);
                //DebugUtils.DrawCircle(center, (center - point).magnitude, rS.direction, Color.cyan);


                //Debug.DrawLine(point, centerStart, Color.magenta);
                //Debug.DrawLine(pointH, centerStart, Color.magenta);


                // TODO: it isn't always shortest angle

                var side = Vector3.Cross(tangent, Vector3.up);
                var dot = Vector3.Dot(side, point - center);

                //if (Mathf.Abs(dot) < 0.0001f)
                //{
                //    side = Vector3.Cross(tangent, Vector3.right);
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
            var det = p3n.sqrMagnitude;
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


            if (Mathf.Abs(dotStart) < 0.1)
            {
                // straight start
                if (Mathf.Abs(dotEnd) < 0.1)
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

            //Debug.DrawRay(t3d(c1), t3d(t1.normalized * 2), Color.gray);
            //Debug.DrawRay(t3d(c2), t3d(t2.normalized * 2), Color.green);

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
                var rc1 = (center1 - start).magnitude;
                // STEP 3: create point H somewhere on the first Circle (we take halfway between start and end)

                var dir = Vector2.Dot(t2, tangentStart)
                    + Vector2.Dot(t2, tangentEnd);

                var distCenter2pointH = (rc1 / t1.magnitude);

                pointH1 = center1 + t1 * distCenter2pointH;
                pointH2 = center1 - t1 * distCenter2pointH;

                //Debug.DrawRay(t3d(c1), t3d(t1.normalized * 2 * Mathf.Sign(u)), Color.red);

                //Debug.DrawLine(t3d(center1), t3d(start), Color.magenta);
                //Debug.DrawLine(t3d(center1), t3d(end), Color.cyan);

                //Debug.DrawLine(t3d(pointH1), t3d(start), Color.yellow);
                //Debug.DrawLine(t3d(pointH1), t3d(end), Color.yellow);
                //Debug.DrawLine(t3d(pointH2), t3d(start), Color.yellow);
                //Debug.DrawLine(t3d(pointH2), t3d(end), Color.yellow);

                //DebugUtils.DrawCircle(t3d(center1), (center1 - start).magnitude, Vector3.up, Color.white);

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

                if (Mathf.Abs(angle) < 0.001)
                    return getLinear(refPoint, pointH, othersegment);
                else
                    return new ArcLoftPath(t3d(arcStartVector), -angle, t3d(center), Vector3.up);
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
            var dot = v1.x * v2.x + v1.y * v2.y;
            var det = v1.x * v2.y - v1.y * v2.x;
            var a = Mathf.Atan2(det, dot);
            if (a < 0) a += Mathf.PI * 2;
            if (!clockWise)
                a = -(Mathf.PI * 2 - a);
            return a;
        }



        private bool intersect(Vector2 p1, Vector2 d1, Vector2 p2, Vector2 d2, out float u, out float v)
        {
            u = (p1.y * d2.x + d2.y * p2.x - p2.y * d2.x - d2.y * p1.x) / (d1.x * d2.y - d1.y * d2.x);
            v = (p1.x + d1.x * u - p2.x) / d2.x;
            return !float.IsInfinity(u);
        }

        private Vector2 makeBiTangent(Vector2 t)
        {
            return new Vector2(t.y, -t.x);
        }
        private Vector3 makeBiTangent(Vector3 t)
        {
            return Vector3.Cross(Vector3.up, t);
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

        public void SnapTo(Vector3 to, out Vector3 position, out float distance)
        {

            Vector3 p1, p2;
            float d1, d2;

             

            arc1.SnapTo(to, out p1, out d1);
            arc2.SnapTo(to, out p2, out d2);
            

            if ((to - p1).sqrMagnitude < (to - p2).sqrMagnitude)
            {
                position = p1;
                distance = d1;
            }
            else
            {
                position = p2;
                distance = d2;
            }
        }
    }
}
