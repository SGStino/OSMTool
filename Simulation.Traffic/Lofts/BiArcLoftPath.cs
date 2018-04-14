
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
        public BiArcLoftPath(Vector3 start, Vector3 forwardStart, Vector3 end, Vector3 forwardEnd)
        {
            var (p0, p1, p2, p3, p4) = BiArcGenerator.Form1(new BiArcParameters(start, forwardStart, end, forwardEnd), 1f);

            if (BiArcGenerator.AreColinear(p0, p1, p2))
                arc1 = new LinearPath(p0, p2);
            else
                arc1 = new ArcLoftPath(BiArcGenerator.ThreePointToCircularArc(p0, p1, p2));
            if (BiArcGenerator.AreColinear(p2, p3, p4))
                arc2 = new LinearPath(p2, p4);
            else
                arc2 = new ArcLoftPath(BiArcGenerator.ThreePointToCircularArc(p2, p3, p4));
        }
          
        

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
