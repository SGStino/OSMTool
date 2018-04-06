using System.Collections.Generic;
using UnityEngine;

namespace Simulation.Traffic.AI.Agents
{
    public interface IAIRouteFinder
    {
       bool Find(Vector3 point, out IReadOnlyList<IAIRoute> routes, out IReadOnlyList<IAIPath> paths);
    }
}