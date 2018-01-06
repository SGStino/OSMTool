using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public interface ILoftPath
    {
        float Length { get; }
        Matrix4x4 GetTransform(float progress);
        void SnapTo(Vector3 to,out Vector3 position, out float distance);
    } 
}
