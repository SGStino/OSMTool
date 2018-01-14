﻿using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI.Navigation
{
    public class PathSolver : GraphSolver<IAIPath>
    {
        public HashSet<IAIPath> allowedPaths;



        public PathSolver(IEnumerable<IAIPath> sources, IEnumerable<IAIPath> destinations, IEnumerable<IAIRoute> limitToRoute) : base(sources, destinations)
        {
            if (limitToRoute != null)
                allowedPaths = new HashSet<IAIPath>(limitToRoute.SelectMany(t => t.Paths));
        }

        public PathSolver(IEnumerable<IAIPath> sources, IEnumerable<IAIPath> destinations) : this(sources, destinations, null)
        {
        }

        protected override float CostBetween(IAIPath current, IAIPath neighbor)
        {
            if (current.RightParralel == neighbor || neighbor.RightParralel == current)
                return 2f; // not totally free
            return neighbor.GetLength() * neighbor.AverageSpeed;
        }

        protected override bool FilterPath(IAIPath item)
        {
            return true;
        }

        protected override float HeuristicCostEstimate(IAIPath s, IAIPath d)
        {
            var start = s.GetEndTransform().GetTranslate();
            var end = d.GetStartTransform().GetTranslate();

            return (end - start).sqrMagnitude;
        }

        protected override IEnumerable<IAIPath> GetNeighbors(IAIPath t)
        {
            if (allowedPaths != null)
                return GetAllNeighbors(t).Where(allowedPaths.Contains);
            return GetAllNeighbors(t);
        }

        private IEnumerable<IAIPath> GetAllNeighbors(IAIPath t)
        {
            if (t.LeftParralel != null)
                yield return t.LeftParralel;
            if (t.RightParralel != null)
                yield return t.RightParralel;
            foreach (var n in t.NextPaths)
                yield return n;
        }
    }
}
