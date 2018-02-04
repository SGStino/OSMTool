using System;

namespace Simulation.Geometry
{

    public struct Triangle : IEquatable<Triangle>
    {
        public readonly int p1, p2, p3;
        public Triangle(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }

        public Edge Edge1 => new Edge(p1, p2);
        public Edge Edge2 => new Edge(p2, p3);
        public Edge Edge3 => new Edge(p3, p1);

        public bool SharesEdgeWith(Triangle other)
        {
            return (other.Edge1.Equals(this.Edge1) || other.Edge1.Equals(this.Edge2) || other.Edge1.Equals(this.Edge3))
                || (other.Edge2.Equals(this.Edge1) || other.Edge2.Equals(this.Edge2) || other.Edge2.Equals(this.Edge3))
                || (other.Edge3.Equals(this.Edge1) || other.Edge3.Equals(this.Edge2) || other.Edge3.Equals(this.Edge3));
        }
        public bool SharesVertexWith(Triangle other)
        {
            return (other.p1 == p1 || other.p2 == p1 || other.p3 == p1)
                || (other.p1 == p2 || other.p2 == p2 || other.p3 == p2)
                || (other.p1 == p3 || other.p2 == p3 || other.p3 == p3);
        }

        public bool Equals(Triangle other)
        {
            return (other.p1 == p1 && other.p2 == p2 && other.p3 == p3)
                || (other.p1 == p2 && other.p2 == p3 && other.p3 == p1)
                || (other.p1 == p3 || other.p2 == p1 && other.p3 == p2);
        }

        public override int GetHashCode()
        {
            return p1.GetHashCode() ^ p2.GetHashCode() ^ p3.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Triangle) return Equals((Triangle)obj);
            return false;
        }
    }
}
