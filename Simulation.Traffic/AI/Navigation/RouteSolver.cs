using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Simulation.Traffic.AI.Navigation
{
    public class RouteSolver : GraphSolver<IAIRoute>
    {
        public RouteSolver(IEnumerable<IAIRoute> sources, IEnumerable<IAIRoute> destinations) :
            base(sources, destinations)
        {
        }

        protected override float CostBetween(IAIRoute current, IAIRoute neighbor)
        {
            var n = neighbor;
            return n.Length / n.Speed * n.Cost;
        }

        protected override bool FilterPath(IAIRoute item)
        {
            return true; // TODO: filter by vehicle types
        }

        protected override float HeuristicCostEstimate(IAIRoute s, IAIRoute d)
        {
            return (s.EndPosition - d.StartPosition).LengthSquared();
        }
    }
}
