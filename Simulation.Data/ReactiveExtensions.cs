using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Data
{

    public struct PreviousState<T>
    {
        public PreviousState(T previous, bool hasPrevious, T current, bool hasCurrent)
        {
            Previous = previous;
            Current = current;
            HasCurrent = hasCurrent;
            HasPrevious = hasPrevious;
        }
        public T Previous { get; }
        public T Current { get; }
        public bool HasCurrent { get; }
        public bool HasPrevious { get; }
    }

    public static class ReactiveExtensions
    {
        public static IObservable<PreviousState<T>> AndPrevious<T>(this IObservable<T> source) => new WithPreviousObservable<T>(source);
    }
    internal class WithPreviousObservable<T> : IObservable<PreviousState<T>>
    {
        private readonly IObservable<T> source;

        public WithPreviousObservable(IObservable<T> source)
        {
            this.source = source;
        }

        public IDisposable Subscribe(IObserver<PreviousState<T>> observer)
        {
            return source.Subscribe(new WithPreviousHandler<T>(observer));
        }
    }
    internal class WithPreviousHandler<T> : IObserver<T>
    {
        bool hasValue = false;
        T currentValue;
        private IObserver<PreviousState<T>> observer;

        public WithPreviousHandler(IObserver<PreviousState<T>> observer)
        {
            this.observer = observer;
        }
        public void OnCompleted()
        {
            observer.OnNext(new PreviousState<T>(currentValue, hasValue, default(T), false));
            observer.OnCompleted(); 
        }

        public void OnError(Exception error)
        {
            observer.OnError(error);
        }

        public void OnNext(T value)
        {
            observer.OnNext(new PreviousState<T>(currentValue, hasValue, value, true));
            hasValue = true;
            currentValue = value;
        }
    }
}
