using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;

namespace Simulation.Data
{
    public interface IObservableValue<out T> : IObservable<T>
    {
        T Value { get; }
    }

    public class BehaviorSubjectValue<T> : IObservableValue<T>, IDisposable
    {
        public BehaviorSubjectValue(IObservable<T> source, T initialValue = default(T))
        {
            subject = new BehaviorSubject<T>(initialValue);
            disposable = new CompositeDisposable()
            {
                source.Subscribe(subject),
                subject
            };
        }
        public BehaviorSubjectValue(T initialValue = default(T))
        {
            subject = new BehaviorSubject<T>(initialValue);
            disposable = new CompositeDisposable()
            {
                subject
            };

        }


        private BehaviorSubject<T> subject;
        private IDisposable disposable;

        public T Value { get => subject.Value; set => subject.OnNext(value); }

        public IDisposable Subscribe(IObserver<T> observer) => subject.Subscribe(observer);

        public void Dispose() => disposable.Dispose();
    }

    public static class BehaviorSubjectValueExtensions
    {
        public static BehaviorSubjectValue<T> ToObservableValue<T>(this IObservable<T> source, T initialValue = default(T)) => new BehaviorSubjectValue<T>(source, initialValue);
    }

}
