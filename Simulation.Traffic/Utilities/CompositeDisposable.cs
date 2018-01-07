using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Utilities
{
    public class CompositeDisposable : IDisposable
    {
        Action dispose;

        public void Dispose() => dispose?.Invoke();

        public void Add(IDisposable disposable) => dispose += disposable.Dispose;

        public void AddRange(IEnumerable<IDisposable> disposables)
        {
            foreach (var disposable in disposables)
                if (disposable != null)
                    Add(disposable);
        }
    }
}
