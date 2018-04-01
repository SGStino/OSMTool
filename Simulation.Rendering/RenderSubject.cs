using System;
using System.Reactive.Subjects;

namespace Simulation.Rendering
{
    internal class RenderSubject : IObservable<(IRenderObject renderObject, RenderEvent renderEvent)>, IDisposable
    {
        private Subject<(IRenderObject renderObject, RenderEvent renderEvent)> subject = new Subject<(IRenderObject renderObject, RenderEvent renderEvent)>();

        public void OnNextEvent(IRenderObject renderObject, RenderEvent renderEvent)
        {
            subject.OnNext((renderObject, renderEvent));
        }

        public IDisposable Subscribe(IObserver<(IRenderObject renderObject, RenderEvent renderEvent)> observer) => subject.Subscribe(observer);

        public void Dispose() => subject.OnCompleted();
    }
}
