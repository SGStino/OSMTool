using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Geometry
{

    public struct Edge : IEquatable<Edge>
    {
        public int p1, p2;
        public Edge(int p1, int p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public bool Equals(Edge other)
        {
            return (other.p1 == p1 && other.p2 == p2) || (other.p1 == p2 && other.p2 == p1);
        }
        public override int GetHashCode()
        {
            return p1.GetHashCode() ^ p2.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Edge) return Equals((Edge)obj);
            return false;
        }
    }

}
