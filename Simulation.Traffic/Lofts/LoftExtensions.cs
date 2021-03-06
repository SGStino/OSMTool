﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public static class LoftExtensions
    {
        public static Vector3 GetTransformedPoint(this ILoftPath loft,  float distance, Vector3 input)
        {
            var m = loft.GetTransform(distance);
            return m.MultiplyPoint3x4(input);
        }

        public static Matrix4x4 GetTransform(this ILoftPath loft, float distance, Matrix4x4 baseTransform)
        {
            return baseTransform * loft.GetTransform(distance);
        }

        public static float DistanceTo(this ILoftPath path, Vector3 point)
        {
            float distance;
            Vector3 snap;
            path.SnapTo(point, out snap, out distance);
            return (point - snap).magnitude;
        }
    }
}
