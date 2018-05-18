using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ClipperLib
{
    public struct Point64
    {
        public Vector<Int64> AsVector() => new Vector<long>(new long[] { X, Y });
        public Int64 X;
        public Int64 Y;
        public Point64(Int64 X, Int64 Y)
        {

            this.X = X; this.Y = Y; 
        }
        public Point64(double x, double y)
        {
            this.X = (Int64)x; this.Y = (Int64)y;
        }

        public Point64(Point64 pt)
        {
            this.X = pt.X; this.Y = pt.Y;
        }

        public static bool operator ==(Point64 a, Point64 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Point64 a, Point64 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj is Point64)
            {
                Point64 a = (Point64)obj;
                return (X == a.X) && (Y == a.Y);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode());
        }

    } //Point64

}
