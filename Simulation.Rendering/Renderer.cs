using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Simulation.Rendering
{ 
    public class Renderer : IObservable<IReadOnlyCollection<KeyValuePair<MaterialInfo, MeshData>>>, IDisposable
    {

        private RenderSubject updates = new RenderSubject();

        private Dictionary<IRenderObject, IDisposable> objects = new Dictionary<IRenderObject, IDisposable>();
        private readonly IObservable<IReadOnlyCollection<KeyValuePair<MaterialInfo, MeshData>>> observable;

        void Add(IRenderObject renderObject)
        {
            void handler(RenderEvent e) => updates.OnNextEvent(renderObject, e);
            objects.Add(renderObject, renderObject.Subscribe(handler));
        }

        public Renderer(IObservable<Unit> sampler)
        {
            observable = updates.Sample(sampler).Select(d => updateRender(d.renderObject, d.renderEvent).ToList().Cast<IReadOnlyCollection<KeyValuePair<MaterialInfo, MeshData>>>()).Switch();
        }


        IObservable<KeyValuePair<MaterialInfo, MeshData>> updateRender(IRenderObject obj, RenderEvent e)
        {
            return Observable.Create<KeyValuePair<MaterialInfo, MeshData>>(observer =>
            { 
                return Scheduler.Default.Schedule(() =>
                {
                    var merged = new Dictionary<MaterialInfo, MeshData>();
                    try
                    {
                        foreach (var renderObj in objects.Keys)
                        {
                            foreach (var material in renderObj.MeshData)
                            {
                                var meshes = material.ToList();
                                if (merged.TryGetValue(material.Key, out var mesh))
                                    meshes.Add(mesh);

                                mesh = MeshData.Merge(meshes);
                                merged[material.Key] = mesh;
                            }
                        }
                        foreach (var mesh in merged)
                            observer.OnNext(mesh);
                    }
                    finally
                    {
                        observer.OnCompleted();
                    }
                });
            });

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

        public void Dispose() => updates.Dispose();

        public IDisposable Subscribe(IObserver<IReadOnlyCollection<KeyValuePair<MaterialInfo, MeshData>>> observer) => observable.Subscribe(observer);
    }
}
