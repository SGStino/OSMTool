using UnityEngine;

namespace Simulation.Data
{
    internal class SimpleSpatialPointer<T> : ISpatialPointer<T>
    {
        private SimpleSpatialRegistry<T> _simpleSpatialRegistry;
        private Rect _bounds;



        public SimpleSpatialPointer(T item, SimpleSpatialRegistry<T> simpleSpatialRegistry, Rect bounds)
        {
            Item = item;
            _bounds = bounds;
            _simpleSpatialRegistry = simpleSpatialRegistry;
        }

        public Rect Bounds
        {
            get => _bounds;
            set
            {
                if (_bounds != value)
                {
                    _simpleSpatialRegistry.Move(this, _bounds, value);
                    _bounds = value;
                }
            }
        }

        public T Item { get; }

        public void Dispose() => _simpleSpatialRegistry.Remove(this, _bounds);
    }
}
