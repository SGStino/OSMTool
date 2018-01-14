using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Traffic.AI.Navigation
{
    public abstract class GraphSolver<T>
        where T : IAIGraphNode

    {
        protected abstract bool FilterPath(T item);

        private class DummySource : IAIGraphNode
        {
            private T[] sources;

            public DummySource(IEnumerable<T> sources)
            {
                this.sources = sources.ToArray();
            }


            public IEnumerable<IAIGraphNode> NextNodes => sources.OfType<IAIGraphNode>();
        }
        protected readonly IAIGraphNode source;
        protected readonly ISet<IAIGraphNode> destination;
        protected readonly ISet<IAIGraphNode> openSet;
        protected readonly ISet<IAIGraphNode> closedSet;
        protected readonly IDictionary<IAIGraphNode, IAIGraphNode> cameFrom;
        protected readonly IDictionary<IAIGraphNode, float> gScore;
        protected readonly IDictionary<IAIGraphNode, float> fScore;
        private int iterations;
        public GraphSolver(IEnumerable<T> sources, IEnumerable<T> destinations)
        {
            this.source = new DummySource(sources);
            destination = new HashSet<IAIGraphNode>(destinations.Cast<IAIGraphNode>());


            openSet = new HashSet<IAIGraphNode>() { source };
            closedSet = new HashSet<IAIGraphNode>();

            cameFrom = new Dictionary<IAIGraphNode, IAIGraphNode>();
            gScore = new Dictionary<IAIGraphNode, float>()
            {
                { source, 0 }
            }
            .WithDefault(k => float.PositiveInfinity);

            fScore = new Dictionary<IAIGraphNode, float>().WithDefault(s => destination.Min(d => heuristic_cost_estimate(s, d)));

            Solution = null;
            IsComplete = false;
            IsSuccess = false;
        }
        public bool Iterate()
        {
            lock (this)
            {
                if (IsComplete || !openSet.Any()) return false;

                ++iterations;

                var current = openSet.OrderBy(o => fScore[o]).First();


                if (destination.Contains(current))
                    return success(current);

                openSet.Remove(current);
                closedSet.Add(current);

                if (!current.NextNodes.Any())
                {

                }

                foreach (var neighbor in getNeighbors(current).Where(filter))
                {
                    if (closedSet.Contains(neighbor)) continue; // already processed
                    openSet.Add(neighbor); // ready to process

                    var tmp_gScore = gScore[current] + cost_between(current, neighbor);

                    if (tmp_gScore >= gScore[neighbor])
                        continue;

                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tmp_gScore;
                    fScore[neighbor] = tmp_gScore + destination.Min(d => heuristic_cost_estimate(neighbor, d));
                }
                if (openSet.Any())
                    return true;
                else
                    return failure();
            }
        }

        private IEnumerable<IAIGraphNode> getNeighbors(IAIGraphNode current)
        {
            if (current is T)
                return GetNeighbors((T)current).OfType<IAIGraphNode>();
            return current.NextNodes;
        }

        protected virtual IEnumerable<T> GetNeighbors(T t)
        {
            return t.NextNodes.OfType<T>();
        }

        private bool filter(IAIGraphNode arg)
        {
            if (arg is T)
                return FilterPath((T)arg);
            return true;
        }

        private float heuristic_cost_estimate(IAIGraphNode neighbor, IAIGraphNode d)
        {
            if (neighbor is T && d is T)
                return HeuristicCostEstimate((T)neighbor, (T)d);
            return float.PositiveInfinity;
        }

        private float cost_between(IAIGraphNode current, IAIGraphNode neighbor)
        {
            if (current is T && neighbor is T)
                return CostBetween((T)current, (T)neighbor);
            return 0;
        }

        private bool failure()
        {
            Solution = null;
            IsComplete = true;
            IsSuccess = false;
            return false;
        }

        private bool success(IAIGraphNode current)
        {
            Solution = reconstruct_path(current);
            IsComplete = true;
            IsSuccess = true;
            return false;
        }

        public IList<T> Solution { get; private set; }

        public bool IsComplete { get; private set; }
        public bool IsSuccess { get; private set; }

        protected abstract float CostBetween(T current, T neighbor);

        private IList<T> reconstruct_path(IAIGraphNode current)
        {
            var path = new List<T>();
            if (current is T)
                path.Add((T)current);
            IAIGraphNode from;
            while (cameFrom.TryGetValue(current, out from))
            {
                current = from;
                if (current is T)
                    path.Add((T)current);
            }
            path.Reverse();
            return path;
        }

        protected abstract float HeuristicCostEstimate(T s, T d);


        public string GetGraph(Func<T, string> getUniqueId)
        {

            var solutionSet = new HashSet<T>(Solution ?? Enumerable.Empty<T>());
            var connections = closedSet.OfType<T>().SelectMany(t =>
           {
               var tN = getUniqueId(t);

               var fromSolution = solutionSet.Contains(t);
               return t.NextNodes.OfType<T>().Select(n =>
               {
                   var toSolution = solutionSet.Contains(n);
                   var l = $"\"{tN}\" -> \"{getUniqueId(n)}\"";

                   if (fromSolution && toSolution)
                       l += $" [color=red]";

                   return l;
               });
           });

            var sb = new StringBuilder();
            sb.AppendLine("digraph G {");
            foreach (var con in connections)
                sb.AppendLine(con);
            sb.AppendLine("}");
            var res = sb.ToString();
            return res;
        }
    }
}
