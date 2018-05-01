using Simulation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
namespace Simulation.Rendering
{
    public static class Renderer
    {
        public static IObservable<IReadOnlyList<MeshData>> Combine(this IObservable<IReadOnlyList<IObservableValue<MeshData>>> source)
        {
            return source.Select(n => n.CombineLatest()).Cast<IReadOnlyList<MeshData>>();
        }

        public static IObservable<MeshData> Merge(this IObservable<IReadOnlyList<MeshData>> data)
            => data.Select(MeshData.Merge);
    }

}
