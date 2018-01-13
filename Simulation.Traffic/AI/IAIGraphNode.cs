using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public interface IAIGraphNode
    {
        IEnumerable<IAIGraphNode> NextNodes { get; }
    }
}
