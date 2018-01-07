using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using System;

namespace Simulation.Traffic
{
    [Obsolete]
    public class FactoryComponent<TResult, TOwner> : RoadComponent<TResult>
        where TResult : class
    {
        private readonly IComponentValueFactory<TResult, TOwner> factory;
        private readonly TOwner owner;

        public FactoryComponent(TOwner owner, IComponentValueFactory<TResult, TOwner> factory)
        {
            this.owner = owner;
            this.factory = factory;
        }
        protected override Task<TResult> GetValueAsync(CancellationToken cancel)
        {
            return factory.CreateAsync(owner, cancel);
        }
    }
}