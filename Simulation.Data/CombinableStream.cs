using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;

namespace Simulation.Data
{

    public class CombinableStream<T> : IObservable<IReadOnlyList<IObservableValue<T>>>
    {
        private HashSet<IObservableValue<T>> data = new HashSet<IObservableValue<T>>();
        private BehaviorSubject<IReadOnlyList<IObservableValue<T>>> subject = new BehaviorSubject<IReadOnlyList<IObservableValue<T>>>(new IObservableValue<T>[0]);

        public void Add(IObservableValue<T> item)
        {
            IObservableValue<T>[] arr = null;
            lock (data)
                if (data.Add(item))
                    arr = data.ToArray();
            if (arr != null)
                subject.OnNext(data.ToArray());
        }
        public void Remove(IObservableValue<T> item)
        {
            IObservableValue<T>[] arr = null;
            lock (data)
                if (data.Add(item))
                    arr = data.ToArray();
            if (arr != null)
                subject.OnNext(data.ToArray());
        }

        public IDisposable Attach(IObservableValue<T> item)
        {
            Add(item);
            return Disposable.Create(() => Remove(item));
        }

        public IDisposable Subscribe(IObserver<IReadOnlyList<IObservableValue<T>>> observer) => subject.Subscribe(observer);
    }

}
