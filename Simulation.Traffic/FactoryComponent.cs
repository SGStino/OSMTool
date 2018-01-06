using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;

namespace Simulation.Traffic
{
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
        protected override Task<TResult> GetValue(CancellationToken cancel)
        {
            return factory.Create(owner, cancel);
        }
    }
}