using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{
    public interface ISegmentAIPathsFactory
    {
        Task<SegmentAIPath[]> CreateAsync(Segment segment, ILoftPath path, CancellationToken cancel);
    }
}