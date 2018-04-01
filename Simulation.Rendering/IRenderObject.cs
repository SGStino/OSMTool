using System;
using System.Linq;

namespace Simulation.Rendering
{
    public interface IRenderObject : IObservable<RenderEvent>
    {
        ILookup<MaterialInfo, MeshData> MeshData { get; }
    }
}
