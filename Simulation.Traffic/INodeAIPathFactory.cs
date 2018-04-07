using Simulation.Traffic.AI;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{

    public interface INodeAIPathsFactory
    {
        NodeAIRoute[] CreateRoutes(ISegmentNodeConnection source, ISegmentNodeConnection target);
    }
}
