using System;
using System.Linq;
using System.Reactive.Linq;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Data
{
    public static class SpatialEventExtensions
    {
        public static IObservable<SpatialEvent<T>> WhereBounds<T>(this IObservable<SpatialEvent<T>> source, Func<Rectangle, bool> predicate)
        {
            if (predicate != null)
                return source.Where(e =>
                {
                    switch (e.Type)
                    {
                        case SpatialEventType.Removed:
                            if (e.OldBounds.HasValue && predicate(e.OldBounds.Value))
                                return true;
                            break;
                        case SpatialEventType.Added:
                            if (e.NewBounds.HasValue && predicate(e.NewBounds.Value))
                                return true;
                            break;
                        case SpatialEventType.Moved:
                            if ((e.NewBounds.HasValue && predicate(e.NewBounds.Value)) || (e.OldBounds.HasValue && predicate(e.OldBounds.Value)))
                                return true;
                            break;
                    }
                    return false;
                });
            return source;
        }
    }
}
