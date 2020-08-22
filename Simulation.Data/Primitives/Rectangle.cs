using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public struct Circle : IEquatable<Circle>
    {
        public Vector2 Center { get; }
        public float Radius { get; }

        public bool Equals(Circle other) => other.Center == Center && other.Radius == Radius;

        public override bool Equals(object obj)
        {
            if (obj is Circle other)
                return Equals(other);
            return false;
        }
        public override int GetHashCode() => Center.GetHashCode() ^ Radius.GetHashCode();
    }
    public struct Rectangle : IEquatable<Rectangle>
    {

        public static Rectangle CenterSize(Vector2 center, Vector2 size)
        {
            size /= 2;
            var min = center - size;
            var max = center + size;

            return MinMaxRectangle(min, max);
        }

        public static Rectangle CornerSize(Vector2 corner, Vector2 size)
        {
            return MinMaxRectangle(corner, corner + size);
        }

        public static Rectangle CornerSize(float x, float y, float w, float h)
        {
            return CornerSize(new Vector2(x, y), new Vector2(w, h));
        }

        public static Rectangle MinMaxRectangle(float x1, float y1, float x2, float y2)
        {
            return MinMaxRectangle(new Vector2(x1, y1), new Vector2(x2, y2));
        }
        public static Rectangle MinMaxRectangle(Vector2 min, Vector2 max)
        {

            var minimum = new Vector2(MathF.Min(min.X, max.X), MathF.Min(min.Y, max.Y));
            var maximum = new Vector2(MathF.Max(min.X, max.X), MathF.Max(min.Y, max.Y));


            return new Rectangle(minimum, maximum);
        }

        public bool Equals(Rectangle other)
        {
            return other.Min == Min && other.Max == Max;
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle rect)
                return Equals(rect);
            return false;
        }

        public Rectangle(Vector2 minimum, Vector2 maximum) : this()
        {
            Min = minimum;
            Max = maximum;
        }
        public Vector2 Min { get; }
        public Vector2 Max { get; }

        public Vector2 Center => (Max + Min) / 2;

        public Vector2 Size => Max - Min;
        public Vector2 Position => Min;

        public static bool operator ==(Rectangle a, Rectangle b) => a.Equals(b);
        public static bool operator !=(Rectangle a, Rectangle b) => !a.Equals(b);

        public float Left => Min.X;
        public float Right => Max.X;
        public float Top => Min.Y;
        public float Bottom => Max.Y;

    }

    public static class RectangleUtils
    {
        public static bool Overlaps(this Rectangle bounds, Circle circle) => Overlaps(bounds, circle.Center, circle.Radius);
        public static bool Overlaps(this Rectangle bounds, Vector2 point, float radius)
        {
            var pX = point.X - MathF.Clamp(point.X, bounds.Min.X, bounds.Max.X);
            var pY = point.Y - MathF.Clamp(point.Y, bounds.Min.Y, bounds.Max.Y);

            var r2 = radius * radius;

            return (pX * pX + pY * pY) <= r2;
        }
        public static bool Overlaps(this Rectangle a, Rectangle b) => !(b.Left > a.Right || b.Right < a.Left || b.Top > a.Bottom || b.Bottom < a.Top);
    }
}
