using Simulation.Traffic.Utilities;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public enum IntersectionMode
    {
        Center,
        Edge
    }
    public static class LoftIntersections2D
    {


        public static bool Intersects(ILoftPath a, float widthA, ILoftPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {
            if (a is LinearPath && b is LinearPath)
                return intersectsLinears(a as LinearPath, widthA, b as LinearPath, widthB, mode, out offsetA, out offsetB);

            if (a is BiArcLoftPath && b is BiArcLoftPath)
                return intersectsBiArcs(a as BiArcLoftPath, widthA, b as BiArcLoftPath, widthB, mode, out offsetA, out offsetB);

            //TODO : arc - arc
            //if (a is ArcLoftPath && b is ArcLoftPath)
            //    return intersectsArcArc(a as ArcLoftPath, widthA, b as ArcLoftPath, widthB, mode, out offsetA, out offsetB);

            //TODO : arc - linear
            //if (a is ArcLoftPath && b is LinearPath)
            //    return intersectsArcLinear(a as ArcLoftPath, widthA, b as LinearPath, widthB, mode, out offsetA, out offsetB);

            //TODO : arc - linear
            //if (a is LinearPath && b is ArcLoftPath)
            //    return intersectsArcLinear(b as ArcLoftPath, widthB, a as LinearPath, widthA, mode, out offsetB, out offsetA);

            //if (intersectsDiscrete(a, widthA, b, widthB, mode, out offsetA, out offsetB))
            //    return true;

            //Debug.LogWarningFormat("combination {0}-{1} not supported ", a.GetType().FullName, b.GetType().FullName);
            offsetB = offsetA = float.NaN;
            return false;
        }

        private static bool intersectsDiscrete(ILoftPath a, float widthA, ILoftPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {
            widthA /= 2;
            widthB /= 2;
            // TODO: optimize!
            var lenA = a.Length;
            var lenB = b.Length;

            var countA = Mathf.CeilToInt(lenA)/4;
            var countB = Mathf.CeilToInt(lenB)/4;



            var dA = lenA / countA;
            var dB = lenB / countB;
            var lTA = a.GetTransform(0);
            for (int iA = 1; iA <= countA; iA++)
            {
                var oA = iA * lenA / countA;
                var tA = a.GetTransform(oA);

                var aLP = t2d(lTA.GetColumn(3));
                var aLT = t2d(lTA.GetColumn(0)) * widthA;
                var aCP = t2d(tA.GetColumn(3));
                var aCT = t2d(tA.GetColumn(0)) * widthA;
                var aD = aCP - aLP;

                lTA = tA;
                var lTB = b.GetTransform(0);
                for (int iB = 1; iB <= countB; iB++)
                {
                    var oB = iB * lenB / countB;
                    var tB = b.GetTransform(oB);


                    var bLP = t2d(lTB.GetColumn(3));
                    var bLT = t2d(lTB.GetColumn(0)) * widthB;
                    var bCP = t2d(tB.GetColumn(3));
                    var bCT = t2d(tB.GetColumn(0)) * widthB;
                    var bD = bCP - bLP;

                    lTB = tB;

                    //Debug.DrawLine(t3d(bLP), t3d(bCP), Color.white);
                    //Debug.DrawLine(t3d(aLP), t3d(aCP), Color.white);

                    //Debug.DrawLine(t3d(bLP + bLT * signB), t3d(bCP + bCT * signB), Color.cyan);
                    //Debug.DrawLine(t3d(aLP + aLT * signA), t3d(aCP + aCT * signA), Color.cyan);

                    //Debug.DrawLine(t3d(bLP - bLT * signB), t3d(bCP - bCT * signB), Color.magenta);
                    //Debug.DrawLine(t3d(aLP - aLT * signA), t3d(aCP - aCT * signA), Color.magenta);



                    var aS1 = aLP + aLT;
                    var bS1 = bLP + bLT;
                    var aS2 = aLP - aLT;
                    var bS2 = bLP - bLT;

                    var aD1 = (aCP + aCT) - aS1;
                    var bD1 = (bCP + bCT) - bS1;
                    var aD2 = (aCP - aCT) - aS2;
                    var bD2 = (bCP - bCT) - bS2;

                    //Debug.DrawLine(t3d(aS1, iA), t3d(aS1 + aD1, iA), Color.cyan);
                    //Debug.DrawLine(t3d(bS1, iA), t3d(bS1 + bD1, iA), Color.cyan);
                    //Debug.DrawLine(t3d(aS2, iA), t3d(aS2 + aD2, iA), Color.magenta);
                    //Debug.DrawLine(t3d(bS2, iA), t3d(bS2 + bD2, iA), Color.magenta);

                    if (mode == IntersectionMode.Edge)
                    {
                        if (intersectsDiscreteSegment(0, a, b, out offsetA, out offsetB, dA, dB, oA, oB, aS1, aD1, bS1, bD1))
                            return true;
                        if (intersectsDiscreteSegment(1, a, b, out offsetA, out offsetB, dA, dB, oA, oB, aS2, aD2, bS2, bD2))
                            return true;
                        if (intersectsDiscreteSegment(2, a, b, out offsetA, out offsetB, dA, dB, oA, oB, aS1, aD1, bS2, bD2))
                            return true;
                        if (intersectsDiscreteSegment(3, a, b, out offsetA, out offsetB, dA, dB, oA, oB, aS2, aD2, bS1, bD1))
                            return true;
                    }
                    else
                    {
                        if (intersectsDiscreteSegment(0, a, b, out offsetA, out offsetB, dA, dB, oA, oB, aLP, bLP, aCP - aLP, bCP - bLP))
                            return true;
                    }
                }
            }
            offsetA = offsetB = float.NaN;
            return false;
        }
        static Color[] colors = { Color.red, Color.green, Color.blue, Color.black };
        private static bool intersectsDiscreteSegment(int slot, ILoftPath a, ILoftPath b, out float offsetA, out float offsetB, float dA, float dB, float oA, float oB, Vector2 aS, Vector2 aD, Vector2 bS, Vector2 bD)
        {
            //int t = Mathf.FloorToInt(Time.time % 4);
            //if (t == slot)
            //{
            //    Debug.DrawLine(t3d(aS), t3d(aS + aD), colors[t]);
            //    Debug.DrawLine(t3d(bS), t3d(bS + bD), Color.cyan);
            //}
            VectorMath2D.IntersectsLineLine(aS, aD, bS, bD, out offsetA, out offsetB);
            if (offsetA >= 0 && offsetA <= 1 && offsetB >= 0 && offsetB <= 1)
            {
                offsetA *= dA;
                offsetB *= dB;
                offsetA += oA - dA;
                offsetB += oB - dB;

                //Debug.DrawLine(b.GetTransformedPoint(offsetB, Vector3.zero), a.GetTransformedPoint(offsetA, Vector3.zero), Color.blue);
                return true;
            }
            offsetB = offsetA = float.NaN;
            return false;
        }

        private static Vector2 t2d(Vector4 input)
        {
            return new Vector2(input.x, input.z);
        }


        private static bool intersectsBiArcs(BiArcLoftPath a, float widthA, BiArcLoftPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {
            if (Intersects(a.Arc1, widthA, b.Arc1, widthB, mode, out offsetA, out offsetB))
                return true;
            if (Intersects(a.Arc2, widthA, b.Arc2, widthB, mode, out offsetA, out offsetB))
            {
                offsetA += a.Arc1.Length;
                offsetB += b.Arc1.Length;
                return true;
            }
            if (Intersects(a.Arc1, widthA, b.Arc2, widthB, mode, out offsetA, out offsetB))
            {
                offsetB += b.Arc1.Length;
                return true;
            }
            if (Intersects(a.Arc2, widthA, b.Arc1, widthB, mode, out offsetA, out offsetB))
            {
                offsetA += a.Arc1.Length;
                return true;
            }
            return false;
        }

        private static bool intersectsLinears(LinearPath a, float widthA, LinearPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {
            var aP = t2d(a.Start);
            var aD = t2d(a.Dir);
            var aT = t2d(a.Tangent);

            var bP = t2d(b.Start);
            var bD = t2d(b.Dir);
            var bT = t2d(b.Tangent);

            widthA /= 2;
            widthB /= 2;

            var lenA = get2DLength(a.Tangent.y, a.Length);
            var lenB = get2DLength(b.Tangent.y, b.Length);

            if (mode == IntersectionMode.Edge)
            {
                var bC = bP + bD * 0.5f;
                var aC = aP + aD * 0.5f;

                var signA = Mathf.Sign(Vector2.Dot(bC - aC, aT));
                var signB = Mathf.Sign(Vector2.Dot(aC - bC, bT));



                if (intersectsRays(aP, aD, aT, signA * widthA, lenA, bP, bD, bT, signB * widthB, lenB, out offsetA, out offsetB))
                    return true;
            }
            else
            {
                if (intersectsRays(aP, aD, aT, 0, lenA, bP, bD, bT, 0, lenB, out offsetA, out offsetB))
                    return true;
            }

            offsetB = offsetA = float.NaN;
            return false;
        }

        private static float get2DLength(float y, float length)
        {
            if (y == 0) return length;
            return Mathf.Sqrt(length * length + y * y);
        }

        private static bool intersectsRays(Vector2 aP, Vector2 aD, Vector2 aT, float widthA, float lenA, Vector2 bP, Vector2 bD, Vector2 bT, float widthB, float lenB, out float offsetA, out float offsetB)
        {
            aP += aT * widthA;
            bP += bT * widthB;

            VectorMath2D.IntersectsLineLine(aP, aD, bP, bD, out offsetA, out offsetB);
            //Debug.DrawLine(t3d(aP), t3d(aP + aD * offsetA, 0), Color.magenta);
            //Debug.DrawLine(t3d(bP), t3d(bP + bD * offsetB, 0), Color.cyan);

            if (offsetA >= 0 && offsetA <= lenA && offsetB >= 0 && offsetB <= lenB)
            {
                //Debug.DrawLine(t3d(aP + aD * offsetA, 0), t3d(aP + aD * offsetA, 10), Color.green);
                return true;
            }
            else
                //Debug.DrawLine(t3d(aP + aD * offsetA, 0), t3d(aP + aD * offsetA, 10), Color.red);
                return false;
        }

        private static Vector2 t2d(Vector3 start)
        {
            return new Vector2(start.x, start.z);
        }
        private static Vector3 t3d(Vector2 start, float offset = 0.5f)
        {
            return new Vector3(start.x, offset, start.y);
        }

        private static bool intersectsArcArc(ArcLoftPath a, float widthA, ArcLoftPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {

            Vector3 aCenter, aAxisX, aAxisY;
            float aRadiusX, aRadiusY;
            if (a.Normal.y > 0.99999)
            {
                aCenter = a.Center;
                aCenter.y = a.StartPoint.y;
                aAxisX = Vector3.right;
                aAxisY = Vector3.forward;
                var ray = a.StartPoint;
                ray.y = 0;
                aRadiusX = aRadiusY = ray.magnitude;
            }
            else
            {
                get2DEllipse(a, out aCenter, out aAxisX, out aAxisY, out aRadiusX, out aRadiusY);
            }

            Vector3 bCenter, bAxisX, bAxisY;
            float bRadiusX, bRadiusY;
            if (b.Normal.y > 0.99999)
            {
                bCenter = b.Center;
                bCenter.y = b.StartPoint.y;
                bAxisX = Vector3.right;
                bAxisY = Vector3.forward;

                var ray = b.StartPoint;
                ray.y = 0;
                bRadiusX = bRadiusY = ray.magnitude;
                Debug.DrawLine(bCenter, b.StartPoint, Color.green);
            }
            else
            {
                get2DEllipse(b, out bCenter, out bAxisX, out bAxisY, out bRadiusX, out bRadiusY);
            }


            var aAngle = -Mathf.Atan2(aAxisX.z, aAxisX.x);
            var bAngle = -Mathf.Atan2(bAxisX.z, bAxisX.x);

            var displayOffset = Vector3.right * 50;

             

            offsetB = offsetA = float.NaN;
            return false;
        }
        private static bool intersectsArcLinear(ArcLoftPath a, float widthA, LinearPath b, float widthB, IntersectionMode mode, out float offsetA, out float offsetB)
        {
            widthA /= 2;
            widthB /= 2;

            if (a.Normal.y > 0.99999f)
            {
                // perfect circle

                var bDirLen = b.Length;
                var bStart = b.Start - a.Center;
                var bDir = b.Dir * bDirLen;
                var bTan = b.Tangent * widthB;

                var center = a.Center;

                center.y = a.StartPoint.y;

                var radius = a.Radius;
                 

                if (mode == IntersectionMode.Edge)
                {
                    if (intersectsEllipseEdge(a, bStart, bDir, bTan, center, bDirLen, widthB, radius, radius, out offsetA, out offsetB))
                    {
                        return true;
                    }
                }
                else
                {
                    if (intersectEllipseCenter(a, out offsetA, out offsetB, center, radius, radius, bDirLen, bStart, bDir))
                        return true;
                }
            }
            else
            {
                Vector3 center, axisX, axisY;
                float radiusX, radiusY;
                get2DEllipse(a, out center, out axisX, out axisY, out radiusX, out radiusY);

                var bDirLen = get2DLength(b.Dir.y, b.Length);
                var bStart = project(b.Start - center, axisX, axisY);
                var bDir = project(b.Dir * bDirLen, axisX, axisY);
                var bTan = project(b.Tangent * widthB, axisX, axisY);

                var aStart = project(a.StartPoint, axisX, axisY);

                //DebugUtils.DrawEllipse(Vector3.zero, radiusX + widthA, radiusY + widthA, Color.cyan);
                //DebugUtils.DrawEllipse(Vector3.zero, radiusX, radiusY, Color.yellow);
                //DebugUtils.DrawEllipse(Vector3.zero, radiusX - widthA, radiusY - widthA, Color.magenta);


                if (mode == IntersectionMode.Edge)
                {
                    if (intersectsEllipseEdge(a, bStart, bDir, bTan, center, bDirLen, widthB, radiusX, radiusY, out offsetA, out offsetB))
                    {
                        return true;
                    }
                }
                else
                {
                    if (intersectEllipseCenter(a, out offsetA, out offsetB, center, radiusX, radiusY, bDirLen, bStart, bDir))
                        return true;
                }
            }

            offsetB = offsetA = float.NaN;
            return false;
        }

        private static void get2DEllipse(ArcLoftPath a, out Vector3 center, out Vector3 axisX, out Vector3 axisY, out float radiusX, out float radiusY)
        {
            var offset = Vector3.Dot(a.StartPoint, a.Normal) * a.Normal;
            center = a.Center + offset;
            var direction = (a.StartPoint + a.Center - center);

            var radius = direction.magnitude;

            axisX = a.Normal;
            axisX.y = 0;
            axisX.Normalize();

            axisY = new Vector3(-axisX.z, 0, axisX.x);
            radiusX = Vector3.Cross(axisX, a.Normal).magnitude * radius;
            radiusY = Vector3.Cross(axisY, a.Normal).magnitude * radius;
            //Debug.DrawLine(center, center + axisX * radiusX, Color.red);
            //Debug.DrawLine(center, center + axisY * radiusY, Color.green);

            //Debug.DrawLine(a.StartPoint + a.Center, a.Center, Color.red);

            //DebugUtils.DrawCircle(center, radius, a.Normal, Color.blue);

            //Debug.DrawLine(center, a.Center, Color.yellow);
        }

        private static bool intersectEllipseCenter(ArcLoftPath a, out float offsetA, out float offsetB, Vector3 center, float radiusX, float radiusY, float bDirLen, Vector3 bStart, Vector3 bDir)
        {
            float nearDistance;
            float farDistance;
            if (VectorMath2D.IntersectsLineEllipse(t2d(bStart), t2d(bDir), Vector3.zero, radiusX, radiusY, out nearDistance, out farDistance))
            {
                float bestOffsetA = float.PositiveInfinity, bestOffsetB = float.PositiveInfinity;
                if (checkRange(a, bDir, center, bDirLen, bStart, nearDistance, ref bestOffsetA, ref bestOffsetB))
                {
                    offsetA = bestOffsetA;
                    offsetB = bestOffsetB;
                    return true;
                }
                if (checkRange(a, bDir, center, bDirLen, bStart, farDistance, ref bestOffsetA, ref bestOffsetB))
                {
                    offsetA = bestOffsetA;
                    offsetB = bestOffsetB;
                    return true;
                }
            }
            offsetA = offsetB = float.NaN;
            return false;
        }

        private static bool intersectsEllipseEdge(ArcLoftPath a, Vector3 bStart, Vector3 bDir, Vector3 bTan, Vector3 center, float bDirLen, float widthB, float radiusX, float radiusY, out float offsetA, out float offsetB)
        {
            var startLeft = bStart - bTan;
            var startRight = bStart + bTan;

            float outerRadiusX = radiusX + widthB;
            float outerRadiusY = radiusY + widthB;
            float innerRadiusX = radiusX - widthB;
            float innerRadiusY = radiusY - widthB;

            float nearDistanceInnerLeft, farDistanceInnerLeft;
            float nearDistanceInnerRight, farDistanceInnerRight;
            float nearDistanceOuterLeft, farDistanceOuterLeft;
            float nearDistanceOuterRight, farDistanceOuterRight;



            bool intersectsInnerLeft = VectorMath2D.IntersectsLineEllipse(t2d(startLeft), t2d(bDir), Vector3.zero, innerRadiusX, innerRadiusY, out nearDistanceInnerLeft, out farDistanceInnerLeft);
            bool intersectsInnerRight = VectorMath2D.IntersectsLineEllipse(t2d(startRight), t2d(bDir), Vector3.zero, innerRadiusX, innerRadiusY, out nearDistanceInnerRight, out farDistanceInnerRight);

            bool intersectsOuterLeft = VectorMath2D.IntersectsLineEllipse(t2d(startLeft), t2d(bDir), Vector3.zero, outerRadiusX, outerRadiusY, out nearDistanceOuterLeft, out farDistanceOuterLeft);
            bool intersectsOuterRight = VectorMath2D.IntersectsLineEllipse(t2d(startRight), t2d(bDir), Vector3.zero, outerRadiusX, outerRadiusY, out nearDistanceOuterRight, out farDistanceOuterRight);


            //if (intersectsOuterLeft)
            //    Debug.DrawLine(startLeft + bDir * nearDistanceOuterLeft, startLeft + bDir * farDistanceOuterLeft, Color.green);
            //if (intersectsOuterRight)
            //    Debug.DrawLine(startRight + bDir * nearDistanceOuterRight, startRight + bDir * farDistanceOuterRight, Color.green);

            //if (intersectsInnerLeft)
            //    Debug.DrawLine(startLeft + bDir * nearDistanceInnerLeft, startLeft + bDir * farDistanceInnerLeft, Color.yellow);
            //if (intersectsInnerRight)
            //    Debug.DrawLine(startRight + bDir * nearDistanceInnerRight, startRight + bDir * farDistanceInnerRight, Color.yellow);


            float bestOffsetA = float.PositiveInfinity, bestOffsetB = float.PositiveInfinity;

            bool any = false;

            if (intersectsOuterLeft)
            {
                any |= checkRange(a, bDir, center, bDirLen, startLeft, nearDistanceOuterLeft, ref bestOffsetA, ref bestOffsetB);
                //any |= checkRange(a, bDir, center, bDirLen, startLeft, farDistanceOuterLeft, ref bestOffsetA, ref bestOffsetB);
            }
            if (intersectsOuterRight)
            {
                any |= checkRange(a, bDir, center, bDirLen, startRight, nearDistanceOuterRight, ref bestOffsetA, ref bestOffsetB);
                //any |= checkRange(a, bDir, center, bDirLen, startRight, farDistanceOuterRight, ref bestOffsetA, ref bestOffsetB);
            }
            if (intersectsInnerLeft)
            {
                //any |= checkRange(a, bDir, center, bDirLen, startLeft, nearDistanceInnerLeft, ref bestOffsetA, ref bestOffsetB);
                any |= checkRange(a, bDir, center, bDirLen, startLeft, farDistanceInnerLeft, ref bestOffsetA, ref bestOffsetB);
            }
            if (intersectsInnerRight)
            {
                //any |= checkRange(a, bDir, center, bDirLen, startRight, nearDistanceInnerRight, ref bestOffsetA, ref bestOffsetB);
                any |= checkRange(a, bDir, center, bDirLen, startRight, farDistanceInnerRight, ref bestOffsetA, ref bestOffsetB);
            }

            if (any)
            {
                offsetA = bestOffsetA;
                offsetB = bestOffsetB;
                return true;
            }


            offsetA = offsetB = float.NaN;
            return false;
        }

        private static bool checkRange(ArcLoftPath a, Vector3 bDir, Vector3 center, float bDirLen, Vector3 startRight, float distance, ref float bestOffsetA, ref float bestOffsetB)
        {
            float offsetA;
            float offsetB;
            if (distance > 0 && distance < 1)
            {
                var farPoint = startRight + bDir * distance + center;
                if (a.GetOffsetToPoint(farPoint, out offsetA))
                {
                    //Debug.DrawLine(farPoint, farPoint + Vector3.left * 5, Color.green);
                    //Debug.DrawLine(farPoint, farPoint + Vector3.forward * 5, Color.green);
                    offsetB = distance * bDirLen;

                    if (offsetA < bestOffsetA)
                        bestOffsetA = offsetA;
                    if (offsetB < bestOffsetB)
                        bestOffsetB = offsetB;
                    return true;
                }
            }
            return false;
        }

        private static Vector3 project(Vector3 input, Vector3 x, Vector3 z)
        {
            var output = new Vector3(0, 0, 0);
            output.x = Vector3.Dot(input, x);
            output.z = Vector3.Dot(input, z);
            return output;
        }
    }
}
