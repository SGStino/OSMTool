
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public class LoftSweep2D
    {
        public static bool Sweep(ILoftPath pathA, float widthA, bool reverseA, ILoftPath pathB, float widthB, bool reverseB, out float offsetA, out float offsetB)
        {
            var lenA = pathA.Length;
            var startA = reverseA ? lenA : 0;
            var endA = reverseA ? 0 : lenA;

            var lenB = pathB.Length;
            var startB = reverseB ? lenB : 0;
            var endB = reverseB ? 0 : lenB;

            return Sweep(pathA, startA, endA, widthA, pathB, startB, endB, widthB, out offsetA, out offsetB);
        }

        private struct Pair
        {
            public int A;
            public int B;

            public Pair(int a, int b)
            {
                A = a;
                B = b;
            }

            public override int GetHashCode()
            {
                return (A + B).GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if (obj is Pair)
                {
                    var pair = (Pair)obj;
                    return (pair.A == A && pair.B == B) || (pair.A == B && pair.B == A);
                }
                return false;
            }
        }
        private static bool Sweep(ILoftPath pathA, float startA, float endA, float widthA, ILoftPath pathB, float startB, float endB, float widthB, out float offsetA, out float offsetB)
        {
            var distA = endA - startA;
            var distB = endB - startB;

            var lenA = Mathf.Abs(distA);
            var lenB = Mathf.Abs(distB);

            var max = Mathf.Max(lenA, lenB);
            var min = Mathf.Min(lenA, lenB);

            var scaleA = Mathf.Sign(distA);
            var scaleB = Mathf.Sign(distB);

            for (int x = 1; x < max+1; x++)
            {
                if (x < lenB)
                    for (int i = 1; i <= Mathf.Min(lenA+1, x+1); i++)
                    {
                        if (test(i, x, pathA, startA, scaleA, widthA, pathB, startB, scaleB, widthB, out offsetA, out offsetB))
                            return true;
                    }
                if (x < lenA)
                    for (int j = 1; j < Mathf.Min(lenB+1, x+1); j++)
                    {
                        if (test(x, j, pathA, startA, scaleA, widthA, pathB, startB, scaleB, widthB, out offsetA, out offsetB))
                            return true;
                    }

                if (x < min)
                {
                    if (test(x, x, pathA, startA, scaleA, widthA, pathB, startB, scaleB, widthB, out offsetA, out offsetB))
                        return true;
                }
            }

            offsetA = float.NaN;
            offsetB = float.NaN;
            return false;
        }

        private static bool test(int a, int b, ILoftPath pathA, float startA, float scaleA, float widthA, ILoftPath pathB, float startB, float scaleB, float widthB, out float offsetA, out float offsetB)
        {
            var sA = startA + (a - 1) * scaleA;
            var eA = startA + (a) * scaleA;
            var sB = startB + (b - 1) * scaleB;
            var eB = startB + (b) * scaleB;


            var sAM = pathA.GetTransform(sA);
            var eAM = pathA.GetTransform(eA);
            var sBM = pathB.GetTransform(sB);
            var eBM = pathB.GetTransform(eB);

            var sAP = getPoint(sAM, 3);
            var eAP = getPoint(eAM, 3);
            var sAT = getPoint(sAM, 0);
            var eAT = getPoint(eAM, 0);

            var sBP = getPoint(sBM, 3);
            var eBP = getPoint(eBM, 3);
            var sBT = getPoint(sBM, 0);
            var eBT = getPoint(eBM, 0);


            var cA = (sAP + eAP) / 2;
            var cB = (sBP + eBP) / 2;

            var tA = cA - cB;
            var tB = -tA;

            var mA = Mathf.Sign(Vector2.Dot(tA, sAT)) * widthA;
            var mB = Mathf.Sign(Vector2.Dot(tB, sBT)) * widthB;

            eAP -= eAT * mA;
            sAP -= sAT * mA;
            eBP -= eBT * mB;
            sBP -= sBT * mB;

            var dA = eAP - sAP;
            var dB = eBP - sBP;
            VectorMath2D.IntersectsLineLine(sAP, dA, sBP, dB, out offsetA, out offsetB);

            //Debug.DrawLine(t3d(sAP), t3d(eAP), Color.magenta);
            //Debug.DrawLine(t3d(sBP), t3d(eBP), Color.cyan);

            //Debug.DrawLine(t3d(cA + sAT), t3d(cA - sAT), Color.red);
            //Debug.DrawLine(t3d(cB + sBT), t3d(cB - sBT), Color.blue);

            //Debug.DrawLine(t3d(cB), t3d(cB), Color.yellow);

            if (offsetA >= 0 && offsetB >= 0 && offsetA <= 1 && offsetB <= 1)
            {

                offsetA = sA + offsetA * scaleA;
                offsetB = sB + offsetB * scaleB;

                //var pA = pathA.GetTransformedPoint(offsetA, Vector3.zero);
                //var pB = pathB.GetTransformedPoint(offsetB, Vector3.zero);
                //Debug.DrawLine(pB, t3d(sAP), Color.red);
                //Debug.DrawLine(pA, t3d(sBP), Color.blue);

                return true;
            }

            offsetA = offsetB = float.NaN;
            return false;
        }

        private static Vector3 t3d(Vector2 sAP, float h = 0f)
        {
            return new Vector3(sAP.x, h, sAP.y);
        }

        private static Vector2 getPoint(Matrix4x4 t, int col)
        {
            var c = t.GetColumn(col);
            return new Vector3(c.x, c.z);
        }
    }
}
