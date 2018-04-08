using Simulation.Data.Primitives;
using System;
using System.Numerics;

namespace Simulation.Data.Trees
{
    public interface IBoundsObject2D
    { 
        IObservableValue<Rectangle> Bounds { get; }
    }
}
