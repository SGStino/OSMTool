using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public struct Ray
    {
        public static Ray BetweenPoints(Vector3 from, Vector3 to)
        {
            return new Ray(from, Vector3.Normalize(to - from));
        }

        public Ray(Vector3 position, Vector3 direction) : this()
        { 
            Origin = position;
            Direction = direction;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
    }
    public static class RayExtensions
    {
        public static Vector3 GetPoint(this Ray ray, float distance)
        {
            return ray.Origin + ray.Direction * distance;
        }
    }
}
