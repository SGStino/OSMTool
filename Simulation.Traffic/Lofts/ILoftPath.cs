using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Lofts
{
    public interface ILoftPath
    {
        float Length { get; }
        Matrix4x4 GetTransform(float progress);
        void SnapTo(Vector3 to, out float distance);
        bool Intersects(Plane plane, out float[] loftDistances);
        Rectangle GetBounds(float width);
    } 
}
