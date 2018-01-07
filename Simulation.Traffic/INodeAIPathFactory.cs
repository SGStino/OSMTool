using Simulation.Traffic.AI;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{
    public interface INodeAIPathFactory
    {
        Task<NodeAIPath[]> CreateAsync(AINode aINode, CancellationToken cancel);
    }
}
