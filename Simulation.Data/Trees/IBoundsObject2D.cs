using System;
using UnityEngine;

namespace Simulation.Data.Trees
{
    public interface IBoundsObject2D
    { 
        IObservableValue<Rect> Bounds { get; }
    }
}
