using Simulation.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        public static ArrayLoftShape CreateFlat(float U1, float U2, float V, float Width, float center)
        {
            var indices = new int[] { 0, 1 };
            var halfWidth = Width / 2;
            var vertices = new LoftVertex[]
            {
                new LoftVertex(){ Normal = Directions2.Up, Position = new Vector2(center-halfWidth, 0), U = U1 },
                new LoftVertex(){ Normal = Directions2.Up, Position = new Vector2(center+halfWidth, 0), U = U2  },
            };

            return new ArrayLoftShape(indices, vertices, V );
        }

        public LoftVertex[] Vertices { get; }

        public float VScale { get; }
    }
}
