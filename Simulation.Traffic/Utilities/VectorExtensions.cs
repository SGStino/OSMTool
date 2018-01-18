using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Utilities
{
    public static class VectorExtensions
    {
        public static Vector3 Round(this Vector3 input)
        {
            return new Vector3(Mathf.Round(input.x), Mathf.Round(input.y), Mathf.Round(input.z));
        }
        public static Vector3 Round(this Vector3 input, float scale)
        {
            input /= scale;
            return scale * new Vector3(Mathf.Round(input.x), Mathf.Round(input.y), Mathf.Round(input.z));
        }
    }
}
