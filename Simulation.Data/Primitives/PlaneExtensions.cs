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
            var denom = Vector3.Dot(plane.Normal, ray.Direction);
            if(denom > 1e-6f)
            {
                var t = Vector3.Dot(plane.Normal * plane.D - ray.Origin, plane.Normal) / denom;
                distance = t;
                return true;
            }
            distance = float.NaN;
            return false;
        }
    }
}
