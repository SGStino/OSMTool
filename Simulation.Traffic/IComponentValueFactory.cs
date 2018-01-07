using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using System;

namespace Simulation.Traffic
{
    [Obsolete]
    public interface IComponentValueFactory<TResult, TOwner>
    {
        Task<TResult> CreateAsync(TOwner owner, CancellationToken cancel);
    }
     
}