using UnityEngine;

namespace Simulation.Traffic.AI.Agents
{
    public interface IAIRouteFinder
    {
       bool Find(Vector3 point, out IAIRoute[] routes, out IAIPath[] paths);
    }
}