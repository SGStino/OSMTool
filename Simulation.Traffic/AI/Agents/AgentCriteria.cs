using System.Collections.Generic;

namespace Simulation.Traffic.AI.Agents
{
    internal class AgentCriteria
    {
        public List<IAIRoute> Routes { get; } = new List<IAIRoute>();
        public List<IAIPath> Paths { get; } = new List<IAIPath>();

        public void Set(IAIRoute[] route, IAIPath[] path)
        {
            Routes.Clear();
            Paths.Clear();
            Routes.AddRange(route);
            Paths.AddRange(path);
        }
    }
}
