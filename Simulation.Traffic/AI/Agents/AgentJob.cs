using Simulation.Traffic.AI.Navigation;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.Traffic.AI.Agents
{
    public class AgentJob : IDisposable // push into agent manager
    {
        public void SetSource(IAIRoute[] route, IAIPath[] path) => startCriteria.Set(route, path);
        public void SetDestination(IAIRoute[] route, IAIPath[] path) => endCriteria.Set(route, path);

        public AgentJob()
        {
        }

        private readonly AgentCriteria startCriteria = new AgentCriteria();
        private readonly AgentCriteria endCriteria = new AgentCriteria();

        private RouteSolver routeSolver;
        private PathSolver pathSolver;

        private AgentJobStatus currentState;

        public void Start()
        {
            StartRouteFinding();
        }


        private Sequence<IAIPath> pathSequence;
        private Sequence<IAIRoute> routeSequence;

        public event Action<AgentJob, AgentJobStatus> StateChanged;

        private void SetState(AgentJobStatus state)
        {
            currentState = state;
            StateChanged?.Invoke(this, state);
        }

        public AgentJobStatus CurrentState => currentState;

        public Sequence<IAIRoute> RouteSequence { get => routeSequence; }
        public Sequence<IAIPath> PathSequence { get => pathSequence; }

        public void Iterate()
        {
            switch (currentState)
            {
                case AgentJobStatus.FindingRoute:
                    if (!routeSolver.Iterate())
                        CompleteRouteFinding();
                    break;
                case AgentJobStatus.FindingPath:
                    if (!pathSolver.Iterate())
                        CompletePathFinding();
                    break;
            }
        }

        private void CompletePathFinding()
        {
            if (pathSolver.IsSuccess)
            {
                pathSequence = new Sequence<IAIPath>(new List<IAIPath>(pathSolver.Solution));
                SetState(AgentJobStatus.Completed);
            }
            else
            {
                SetState(AgentJobStatus.PathNotFound);
            }
        }

        private void CompleteRouteFinding()
        {
            if (routeSolver.IsSuccess)
            {
                routeSequence = new Sequence<IAIRoute>(new List<IAIRoute>(routeSolver.Solution));
                StartPathFinding();
            }
            else
            {
                SetState(AgentJobStatus.RouteNotFound);
            }
            routeSolver = null;
        }

        private void StartRouteFinding()
        {
            if (endCriteria.Routes?.Any() ?? false)
            {
                routeSolver = new RouteSolver(startCriteria.Routes, endCriteria.Routes);
                SetState(AgentJobStatus.FindingRoute);
            }
            else
            {
                SetState(AgentJobStatus.RouteNotFound);
            }
        }
        private void StartPathFinding()
        {
            var sourcePaths = GetSourcePaths();
            var destinationPaths = GetDestinationPaths();

            if (destinationPaths?.Any() ?? false)
            {
                pathSolver = new PathSolver(sourcePaths, destinationPaths, RouteSequence);

                SetState(AgentJobStatus.FindingPath);
            }
            else
            {
                SetState(AgentJobStatus.PathNotFound);
            }
        }

        protected virtual IEnumerable<IAIPath> GetDestinationPaths() =>
            RouteSequence?.Any() ?? false
            ? endCriteria.Paths != null
                ? endCriteria.Paths.Intersect(RouteSequence.Last().Paths)
                : RouteSequence.LastOrDefault()?.Paths
            : null;


        protected virtual IEnumerable<IAIPath> GetSourcePaths() =>
            RouteSequence?.Any() ?? false
            ? startCriteria?.Paths != null
                    ? RouteSequence.CurrentItem.Paths.Intersect(startCriteria.Paths)
                    : RouteSequence.CurrentItem.Paths
            : null;

        public void Dispose()
        {
            SetState(AgentJobStatus.Cancelled);
        }
    }
}
