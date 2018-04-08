using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Simulation.Traffic.Utilities
{
    public static class VectorExtensions
    {
        public static Vector3 Round(this Vector3 input)
        {
            return new Vector3(MathF.Round(input.X), MathF.Round(input.Y), MathF.Round(input.Z));
        }
        public static Vector3 Round(this Vector3 input, float scale)
        {
            input /= scale;
            return scale * new Vector3(MathF.Round(input.X), MathF.Round(input.Y), MathF.Round(input.Z));
        }
    }
}
