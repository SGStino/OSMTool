using System;
using System.Collections.Generic;
using System.Reactive.Linq;
namespace Simulation.Rendering
{
    public class Renderer
    {
        public Dictionary<IRenderObject, IDisposable> objects = new Dictionary<IRenderObject, IDisposable>();

        void Add(IRenderObject renderObject)
        {
            objects.Add(renderObject, renderObject.Subscribe(e => updateRender(renderObject, e)));
        }

        void updateRender(IRenderObject obj, RenderEvent e)
        {

        }


        bool Remove(IRenderObject renderObject)
        {
            if (objects.TryGetValue(renderObject, out var value))
            {
                value.Dispose();
                return true;
            }
            return false;
        }
    }

    internal interface IRenderObject : IObservable<RenderEvent>
    {
        MeshData[] MeshData { get; }
    }

    public class MeshData
    {
    }

    internal struct RenderEvent
    {
    }
}
