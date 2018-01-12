using Simulation.Traffic.AI;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{

    public interface INodeAIPathsFactory
    {
        NodeAIRoute[] CreateRoutes(AISegmentNodeConnection node);
    }
}
