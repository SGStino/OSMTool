using System;
using System.Text;
using UnityEngine;

namespace Simulation.Data
{

    public interface ISpatialPointer<T> : IDisposable
    {
        Rect Bounds { get; set; }
        T Item { get; }
    }
}
