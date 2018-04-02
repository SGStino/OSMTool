using System;
using UnityEngine;

namespace Simulation.Data.Trees
{
    public interface IBoundsObject2D
    { 
        IObservable<Rect> Bounds { get; }
    }
}
