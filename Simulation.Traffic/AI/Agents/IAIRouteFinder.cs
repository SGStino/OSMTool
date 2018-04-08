using System.Collections.Generic;
using System.Numerics;

namespace Simulation.Traffic.AI.Agents
{
    public interface IAIRouteFinder
    {
       bool Find(Vector3 point, out IReadOnlyList<IAIRoute> routes, out IReadOnlyList<IAIPath> paths);
    }
}