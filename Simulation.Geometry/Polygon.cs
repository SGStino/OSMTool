using ClipperLib;
using Simulation.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Geometry
{
    public static class Polygon
    {
        public static Vector2[] ExpandNormal(this IReadOnlyList<Vector2> input, params float[] amount)
        {
            var c = input.Count;
            var output = new Vector2[c];

            Parallel.For(0, c, i =>
            {
                var i0 = i;
                var i1 = (i + 1) % c;
                var i2 = (i + 2) % c;

                var p0 = input[i0];
                var p1 = input[i1];
                var p2 = input[i2];

                var n1 = p0 - p1;
                var n2 = p1 - p2;


                n1 = new Vector2(-n1.Y, n1.X) / n1.Length();
                n2 = new Vector2(-n2.Y, n2.X) / n2.Length();

                var n = Vector2.Normalize(n1 + n2);
                output[i1] = input[i1] + n * amount[i % amount.Length];
            });
            return output;
        }
        public static Vector2[] ExpandEdge(this IReadOnlyList<Vector2> input, float delta)
        {
            var bounds = GetBounds(input);

            var intPoints = new IntPoint[input.Count];

            var center = bounds.Center;
            GetScalingFactor(bounds, delta, out var size, out var max);
            Parallel.For(0, input.Count, i => intPoints[i] = ScaleToInt(input[i], center, size, max));



            ClipperOffset co = new ClipperOffset();
            co.AddPath(new List<IntPoint>(intPoints), JoinType.jtMiter, EndType.etClosedPolygon);
            List<List<IntPoint>> resized = new List<List<IntPoint>>();
            co.Execute(ref resized, delta / size * max);
            //throw new InvalidCastException(bounds.Max + " " + bounds.Min + " " + size + " resized:" + resized.Count + " " + string.Join(" ", intPoints.Select(n => n.X + ", " + n.Y)));
            var result = new Vector2[resized[0].Count];
            Parallel.For(0, result.Length, i => result[i] = ScaleFromInt(resized[0][i], center, size, max));
            return result;
        }


        public static Vector2[] Intersect(this IReadOnlyList<Vector2> input, IReadOnlyList<Vector2> clip)
        {
            var bounds1 = GetBounds(input);
            var bounds2 = GetBounds(clip);
            var bounds = Rectangle.MinMaxRectangle(Vector2.Min(bounds1.Min, bounds2.Min), Vector2.Max(bounds1.Max, bounds2.Max));

            GetScalingFactor(bounds, 0, out var size, out var max);
            var a = input.Select(i => ScaleToInt(i, bounds.Center, size, max)).ToList();
            var b = clip.Select(i => ScaleToInt(i, bounds.Center, size, max)).ToList();

            Clipper c = new Clipper();
            c.AddPath(a, PolyType.ptSubject, true);
            c.AddPath(b, PolyType.ptClip, true);
            var r = new List<List<IntPoint>>() { new List<IntPoint>() };
            c.Execute(ClipType.ctXor, r, PolyFillType.pftNonZero);

            var result = new Vector2[r[0].Count];
            Parallel.For(0, result.Length, i => result[i] = ScaleFromInt(r[0][i], bounds.Center, size, max));
            return result;
        }

        private static Vector2 ScaleFromInt(IntPoint intVector, Vector2 center, float sizeUniform, long max)
        {
            var x = intVector.X * sizeUniform / max + center.X;
            var y = intVector.Y * sizeUniform / max + center.Y;
            return new Vector2(x, y);
        }

        private static IntPoint ScaleToInt(Vector2 vector2, Vector2 center, float sizeUniform, long max)
        {
            var x = (long)((vector2.X - center.X) / sizeUniform * max);
            var y = (long)((vector2.Y - center.Y) / sizeUniform * max);
            return new IntPoint(x, y);
        }

        private static void GetScalingFactor(Rectangle bounds, float delta, out float sizeUniform, out long max)
        {
            var size = bounds.Size;
            sizeUniform = Math.Max(size.X, size.Y) + Math.Max(0, delta);
            max = Int32.MaxValue;// MathF.RoundToInt(sizeUniform) * 1000;
        }

        private static Rectangle GetBounds(IReadOnlyList<Vector2> input)
        {
            Vector2 min = input[0], max = input[0];
            for (int i = 1; i < input.Count; i++)
            {
                max = Vector2.Max(max, input[i]);
                min = Vector2.Min(min, input[i]);
            }
            return Rectangle.MinMaxRectangle(min, max);
        }

        public static bool Contains(this IReadOnlyList<Vector2> polygon, Vector2 point)
        {
            double minX = polygon[0].X;
            double maxX = polygon[0].X;
            double minY = polygon[0].Y;
            double maxY = polygon[0].Y;
            for (int i = 1; i < polygon.Count; i++)
            {
                var q = polygon[i];
                minX = Math.Min(q.X, minX);
                maxX = Math.Max(q.X, maxX);
                minY = Math.Min(q.Y, minY);
                maxY = Math.Max(q.Y, maxY);
            }

            if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY)
            {
                return false;
            }

            // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if ((polygon[i].Y > point.Y) != (polygon[j].Y > point.Y) &&
                     point.X < (polygon[j].X - polygon[i].X) * (point.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) + polygon[i].X)
                {
                    inside = !inside;
                }
            }

            return inside;

        }
    }
}
