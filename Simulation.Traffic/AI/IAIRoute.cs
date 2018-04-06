using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public interface IAIRoute : IAIGraphNode
    {
        Vector3 StartPosition { get; }
        Vector3 EndPosition { get; }
        float Length { get; }
        float Speed { get; }
        float Cost { get; }
        IReadOnlyList<IAIPath> Paths { get; }
        IEnumerable<IAIRoute> NextRoutes { get; }
    }
}
