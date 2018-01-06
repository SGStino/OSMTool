using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;

namespace Simulation.Traffic
{
    public interface IComponentValueFactory<TResult, TOwner>
    {
        Task<TResult> Create(TOwner owner, CancellationToken cancel);
    }
}