using System;
using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Data
{
    public interface ISpatialRegistry<T> : IDisposable
    {
        IEnumerable<T> Query(Rect area);
        IEnumerable<T> Query(Vector3 center, float radius);
        IObservable<SpatialEvent<T>> Observe(Rect area);
        IObservable<SpatialEvent<T>> Observe(Vector3 center, float radius);
        ISpatialPointer<T> Register(T item, Rect bounds);
    }
}
