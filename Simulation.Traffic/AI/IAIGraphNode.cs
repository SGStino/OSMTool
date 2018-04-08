using System.Collections.Generic;
using System.Numerics;

namespace Simulation.Traffic.AI
{
    public interface IAIGraphNode
    {
        IEnumerable<IAIGraphNode> NextNodes { get; }
    }
}
