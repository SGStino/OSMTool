using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.Lofts
{
    public class ArrayLoftShape : ILoftShape
    {
        public ArrayLoftShape(int[] indices, LoftVertex[] vertices, float vscale)
        {
            Indices = indices;
            Vertices = vertices;
            VScale = vscale;
        }

        public int[] Indices { get; private set; }

        public LoftVertex[] Vertices { get; private set; }

        public float VScale { get; private set; }
    }
}
