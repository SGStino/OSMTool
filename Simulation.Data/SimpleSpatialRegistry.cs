using Simulation.Data.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Simulation.Data
{
    public class SimpleSpatialRegistry<T> : ISpatialRegistry<T>
    {
        Subject<SpatialEvent<T>> localEvents = new Subject<SpatialEvent<T>>();
        private HashSet<ISpatialPointer<T>> pointers = new HashSet<ISpatialPointer<T>>();

        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            foreach (var pointer in pointers)
                localEvents.OnNext(SpatialEvent<T>.Removed(pointer.Item, pointer.Bounds));
            pointers.Clear();
        }

        public IObservable<SpatialEvent<T>> Observe(Rect area) => localEvents.WhereBounds(b => b.Overlaps(area));
        public IObservable<SpatialEvent<T>> Observe(Vector3 center, float radius) => localEvents.WhereBounds(b => QuadTreeUtils.Overlaps(b, center, radius));

        public IEnumerable<T> Query(Rect area) => pointers.Where(b => b.Bounds.Overlaps(area)).Select(t => t.Item);

        public IEnumerable<T> Query(Vector3 center, float radius) => pointers.Where(b => QuadTreeUtils.Overlaps(b.Bounds, center, radius)).Select(t => t.Item);

        public ISpatialPointer<T> Register(T item, Rect bounds)
        {
            var pointer = new SimpleSpatialPointer<T>(item, this, bounds);
            pointers.Add(pointer);
            localEvents.OnNext(SpatialEvent<T>.Added(item, bounds));
            return pointer;
        }

        internal void Move(SimpleSpatialPointer<T> simpleSpatialPointer, Rect oldBounds, Rect newBounds) => localEvents.OnNext(SpatialEvent<T>.Moved(simpleSpatialPointer.Item, oldBounds, newBounds));

        internal void Remove(SimpleSpatialPointer<T> simpleSpatialPointer, Rect bounds)
        {
            localEvents.OnNext(SpatialEvent<T>.Removed(simpleSpatialPointer.Item, bounds));
            pointers.Remove(simpleSpatialPointer);
        }
    }
}
