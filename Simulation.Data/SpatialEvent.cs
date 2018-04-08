using Simulation.Data.Primitives;
using System.Numerics;

namespace Simulation.Data
{
    public struct SpatialEvent<T>
    {
        public SpatialEvent(T item, Rectangle? oldBounds, Rectangle? newBounds, SpatialEventType type) : this()
        {
            Item = item;
            OldBounds = oldBounds;
            NewBounds = newBounds;
            Type = type;
        }

        public T Item { get; }
        public Rectangle? OldBounds { get; }
        public Rectangle? NewBounds { get; }
        public SpatialEventType Type { get; }


        public static SpatialEvent<T> Added(T item, Rectangle bounds) => new SpatialEvent<T>(item, null, bounds, SpatialEventType.Added);
        public static SpatialEvent<T> Removed(T item, Rectangle bounds) => new SpatialEvent<T>(item, bounds, null, SpatialEventType.Removed);
        public static SpatialEvent<T> Moved(T item, Rectangle oldBounds, Rectangle newBounds) => new SpatialEvent<T>(item, oldBounds, newBounds, SpatialEventType.Moved);
    }
}
