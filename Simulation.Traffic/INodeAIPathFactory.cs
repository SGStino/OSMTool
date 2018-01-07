using Simulation.Traffic.AI;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{
    public interface INodeAIPathFactory
    {
        NodeAIPath[] Create(AINode aINode);
    }
}
