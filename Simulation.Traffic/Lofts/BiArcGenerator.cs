using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static System.Numerics.Vector3;
using static System.MathF;
using MathNet.Numerics.RootFinding;
using MathNet.Numerics;
using System.Linq;

namespace Simulation.Traffic.Lofts
{
    public struct BiArcParameters
    {
        public Vector3 P1, T1, P2, T2;

        public BiArcParameters(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2)
        {
            P1 = p1;
            T1 = t1;
            P2 = p2;
            T2 = t2;
        }

        public void Deconstruct(out Vector3 p1, out Vector3 t1, out Vector3 p2, out Vector3 t2)
        {
            p1 = P1;
            p2 = P2;
            t1 = T1;
            t2 = T2;
        }
    }

    public struct ArcDefinition
    {
        public Vector3 Center, E1, E2, EndDir, Axis;
        public float R, Theta;

        public ArcDefinition(Vector3 center, Vector3 e1, Vector3 e2, Vector3 endDir, Vector3 axis, float r, float theta)
        {
            Center = center;
            E1 = e1;
            E2 = e2;
            EndDir = endDir;
            Axis = axis;
            R = r;
            Theta = theta;
        }
    }


    public static class BiArcGenerator
    {
        public static ArcDefinition ThreePointToCircularArc(Vector3 A, Vector3 B, Vector3 c)
        {
            var AB = B - A;
            var BC = c - B;

            var lenAB = AB.Length();
            var lenBC = BC.Length();

            var ab = AB / lenAB;
            var bc = BC / lenBC;

            var cos2t = Dot(ab, bc);
            var rot = Cross(ab, bc);
            var sin2t = rot.Length();
            var axis = rot / sin2t;
            var tant = Sqrt((1 - cos2t) / (1 + cos2t));
            var r = lenAB / tant;
            var theta = Atan2(sin2t, cos2t) / 2;

            var mid = Normalize(BC - AB);
            var center = B + (r / Cos(theta)) * mid;

            var e1 = Normalize(A - center);
            var e2 = Normalize(Cross(axis, e1));

            var enddir = e1 * cos2t + e2 * sin2t;
            return new ArcDefinition(center: center, e1: e1, e2: e2, endDir: enddir, axis: axis, r: r, theta: theta);
        }

        public static (ArcDefinition arc1, ArcDefinition arc2) Form1(BiArcParameters parameters, float rho)
        {
            float a1, a2;
            Vector3 B1, B2;
            Vector3 B12;
            var (p1, t1, p2, t2) = parameters;

            var delta = p1 - p2;
            var C2 = rho * (Dot(t1, t2) - 1);
            var C1 = Dot(delta, t1) + rho * Dot(delta, t2);
            var C0 = Dot(delta, delta) / 2;

            var (r1, r2) = FindRoots.Quadratic(C0, C1, C2);

            a1 = new[] { (float)r1.Real, (float)r2.Real }.Where(n => n > 0).Min();

            a2 = rho * a1;

            var check = (delta + a1 * t1 + a2 * t2).Length() - a1 - a2;

            B1 = p1 + a1 * t1;
            B2 = p2 - a2 * t2;

            B12 = (a2 * B1 + a1 * B2) / (a1 + a2);

            var arc1 = ThreePointToCircularArc(p1, B1, B12);
            var arc2 = ThreePointToCircularArc(B12, B2, p2);
            return (arc1, arc2);
        }

    }

}
