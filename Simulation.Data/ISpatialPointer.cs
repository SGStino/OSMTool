using System;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Data
{

    public interface ISpatialPointer<T> : IDisposable
    {  
        Rectangle Bounds { get; set; }
        T Item { get; }
    }
}
