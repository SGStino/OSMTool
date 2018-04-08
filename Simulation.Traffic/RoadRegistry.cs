using Simulation.Data;
using Simulation.Data.Trees;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic
{
 

    public class BoundsObjects2DRegistration<T> : IDisposable
    {
        public ISpatialPointer<T> Pointer { get; }

        private IDisposable disposable;

        public T Node { get; }

        public BoundsObjects2DRegistration(T node, ISpatialPointer<T> pointer, IObservable<Rectangle> observable)
        {
            Node = node;
            Pointer = pointer;
            disposable = observable.Subscribe(updateBounds);
        }

        void updateBounds(Rectangle evt)
        {
            Pointer.Bounds = evt;
        }

        public void Dispose()
        {
            Pointer.Dispose();
            disposable.Dispose();
            //Disposed?.Invoke(this);
            //Disposed = null;
        }

        //public event Action<BoundsObjects2DRegistration<T>> Disposed;
    }

    public class BoundsObjects2DRegistry<T> where T : IBoundsObject2D
    {
        public BoundsObjects2DRegistry(Func<T, Rectangle> getNode)
        { }

        private ISpatialRegistry<T> spatialRegistry = new SimpleSpatialRegistry<T>();
        //private Dictionary<T, BoundsObjects2DRegistration<T>> nodeRegistrations = new Dictionary<T, BoundsObjects2DRegistration<T>>();

        public BoundsObjects2DRegistration<T> Register(T node)
        { 
            var pointer = spatialRegistry.Register(node, node.Bounds.Value); 
            var registration = new BoundsObjects2DRegistration<T>(node, pointer, node.Bounds);
            //nodeRegistrations.Add(node, registration);

            //registration.Disposed += Remove;

            return registration;
        }

        //private void Remove(BoundsObjects2DRegistration<T> obj)
        //{
        //    nodeRegistrations.Remove(obj.Node); 
        //}
    }
}
