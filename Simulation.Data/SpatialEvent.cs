using UnityEngine;

namespace Simulation.Data
{
    public struct SpatialEvent<T>
    {
        public SpatialEvent(T item, Rect? oldBounds, Rect? newBounds, SpatialEventType type) : this()
        {
            Item = item;
            OldBounds = oldBounds;
            NewBounds = newBounds;
            Type = type;
        }

        public T Item { get; }
        public Rect? OldBounds { get; }
        public Rect? NewBounds { get; }
        public SpatialEventType Type { get; }


        public static SpatialEvent<T> Added(T item, Rect bounds) => new SpatialEvent<T>(item, null, bounds, SpatialEventType.Added);
        public static SpatialEvent<T> Removed(T item, Rect bounds) => new SpatialEvent<T>(item, bounds, null, SpatialEventType.Removed);
        public static SpatialEvent<T> Moved(T item, Rect oldBounds, Rect newBounds) => new SpatialEvent<T>(item, oldBounds, newBounds, SpatialEventType.Moved);
    }
}
