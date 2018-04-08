using Simulation.Data.Primitives;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Simulation.Data
{
    public interface ISpatialRegistry<T> : IDisposable
    {
        IEnumerable<T> Query(Rectangle area);
        IEnumerable<T> Query(Vector2 center, float radius);
        IObservable<SpatialEvent<T>> Observe(Rectangle area);
        IObservable<SpatialEvent<T>> Observe(Vector2 center, float radius);
        ISpatialPointer<T> Register(T item, Rectangle bounds);
    }
}
