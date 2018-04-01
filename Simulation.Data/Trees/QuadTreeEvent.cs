using System.Collections.Generic;
using System.Linq;

namespace Simulation.Data.Trees
{
    public struct QuadTreeEvent<T>
    {
        public QuadTreeEvent(T item, QuadTreeEventType type) : this()
        {
            Item = item;
            Type = type;
        }
         
        public T Item { get; }
        public QuadTreeEventType Type { get; }

        internal static QuadTreeEvent<T> Removed(T item)
        {
            return new QuadTreeEvent<T>(item, QuadTreeEventType.Removed);
        }
        internal static QuadTreeEvent<T> Added(T item)
        {
            return new QuadTreeEvent<T>(item, QuadTreeEventType.Added);
        }
    }

    public enum QuadTreeEventType
    {
        Added,
        Removed
    }
}
