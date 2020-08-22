using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public static class PlaneExtensions
    {
        public static bool Raycast(this Plane plane, Ray ray, out float distance)
        {
            var u = ray.Direction;

            var denom = (Vector3.Dot(plane.Normal, ray.Direction));
            if (Math.Abs(denom) > 1e-6f)
            {
                var co = plane.Normal * (-plane.D / plane.Normal.LengthSquared());

                var w = ray.Origin - co;

                var t = -Vector3.Dot(plane.Normal, w) / denom;
                //var t = Vector3.Dot(plane.Normal * plane.D - ray.Origin, plane.Normal) / denom;
                distance = t;
                return true;
            }
            distance = float.NaN;
            return false;
        }
    }
}
