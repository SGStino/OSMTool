using Simulation.Traffic.AI.Navigation;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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



    public enum AgentState
    {
        FindingRoute = 1,
        FindingPath = 2,
        Completed = 4,
        Error = 8,
        RouteNotFound = 16 | Error,
        PathNotFound = 32 | Error,
        Cancelled = 64 | Error
    }

    public class AgentJob : IDisposable // push into agent manager
    {
        public void SetSource(IAIRoute[] route, IAIPath[] path) => startCriteria.Set(route, path);
        public void SetDestination(IAIRoute[] route, IAIPath[] path) => endCriteria.Set(route, path);

        public AgentJob(Action<Sequence<IAIPath>> pathCallback, Action<Sequence<IAIRoute>> routeCallback)
        {
            this.pathCallback = pathCallback;
            this.routeCallback = routeCallback;
        }

        private readonly AgentCriteria startCriteria = new AgentCriteria();
        private readonly AgentCriteria endCriteria = new AgentCriteria();

        private RouteSolver routeSolver;
        private PathSolver pathSolver;
        private readonly Action<Sequence<IAIPath>> pathCallback;
        private readonly Action<Sequence<IAIRoute>> routeCallback;

        private AgentState currentState;

        public void Start()
        {
            StartRouteFinding();
        }


        private Sequence<IAIPath> pathSequence;
        private Sequence<IAIRoute> routeSequence;

        public event Action<AgentJob, AgentState> StateChanged;

        private void SetState(AgentState state)
        {
            currentState = state;
            StateChanged?.Invoke(this, state);
        }

        public AgentState CurrentState => currentState;

        public void Iterate()
        {
            switch (currentState)
            {
                case AgentState.FindingRoute:
                    if (!routeSolver.Iterate())
                        CompleteRouteFinding();
                    break;
                case AgentState.FindingPath:
                    if (!pathSolver.Iterate())
                        CompletePathFinding();
                    break;
            }
        }

        private void CompletePathFinding()
        {
            if (pathSolver.IsSuccess)
            {
                pathCallback(pathSequence = new Sequence<IAIPath>(new List<IAIPath>(pathSolver.Solution)));
                SetState(AgentState.Completed);
            }
            else
            {
                SetState(AgentState.PathNotFound);
            }
        }

        private void CompleteRouteFinding()
        {
            if (routeSolver.IsSuccess)
            {
                routeCallback(routeSequence = new Sequence<IAIRoute>(new List<IAIRoute>(routeSolver.Solution)));
                StartPathFinding();
            }
            else
            {
                SetState(AgentState.RouteNotFound);
            }
            routeSolver = null;
        }

        private void StartRouteFinding()
        {
            routeSolver = new RouteSolver(startCriteria.Routes, endCriteria.Routes);
            SetState(AgentState.FindingRoute);
        }
        private void StartPathFinding()
        {
            var sourcePaths = GetSourcePaths();
            var destinationPaths = GetDestinationPaths();

            pathSolver = new PathSolver(sourcePaths, destinationPaths, routeSequence);

            SetState(AgentState.FindingPath);
        }

        protected virtual IEnumerable<IAIPath> GetDestinationPaths() =>
            endCriteria.Paths != null
                ? endCriteria.Paths.Intersect(routeSequence.Last().Paths)
                : routeSequence.Last().Paths;

        protected virtual IEnumerable<IAIPath> GetSourcePaths() =>
            startCriteria?.Paths != null
                ? routeSequence.CurrentItem.Paths.Intersect(startCriteria.Paths)
                : routeSequence.CurrentItem.Paths;

        public void Dispose()
        {
            SetState(AgentState.Cancelled);
        }
    }



    public class Agent
    {

        private Sequence<IAIRoute> routeSequence;
        private Sequence<IAIPath> pathSequence;

        private readonly AgentJob solverJob;
        private readonly IAIRouteFinder finder;
        private readonly IAgentJobRunner runner;

        public Agent(IAIRouteFinder finder, IAgentJobRunner runner)
        {
            solverJob = new AgentJob(pathUpdated, routeUpdated);
            this.finder = finder;
            this.runner = runner;
        }

        private void pathUpdated(Sequence<IAIPath> obj)
        {
            pathSequence = obj;
        }

        private void routeUpdated(Sequence<IAIRoute> obj)
        {
            routeSequence = obj;
        }

        public void Update(float dt)
        {
             if(pathSequence != null)
            {
                progress += dt;
                var pathLength = pathSequence.CurrentItem.GetLength();
                if (progress > pathLength)
                {
                    isLastKnownTransformValid = false;
                    progress -= pathLength;
                    if (!pathSequence.Next())
                    {
                        DestinationReached();
                        pathSequence = null;
                    }
                }
            }
        }

        protected virtual void DestinationReached()
        {

        }

        private float progress;

        private Matrix4x4 lastKnownTransform;
        private bool isLastKnownTransformValid;


        public Matrix4x4 GetTransform => isLastKnownTransformValid ? lastKnownTransform : throw new NotImplementedException();//pathSequence?.CurrentItem.LoftPath.GetTransform(progress, getBaseTransform(progress));

   

        public void Teleport(Matrix4x4 start)
        {
            lastKnownTransform = start;
            isLastKnownTransformValid = true;
            var startPosition = start.GetTranslate();
            if (finder.Find(startPosition, out var routes, out var paths))
            {
                solverJob.SetSource(routes, paths);
                Activate(solverJob);
            }
        }

        private void Activate(AgentJob solverJob)
        {
            solverJob.Start();
            runner.Run(solverJob);
        }

        //private bool GoToNextPath()
        //{
        //    return routeSequence?.Next() ?? false;
        //}

        //public Matrix4x4 GetTransform() => currentPath.LoftPath.GetTransform(PathProgress);


        //public void SetRoute(IAIRoute[] route, int index = 0)
        //{
        //    this.routeSequence = new Sequence<IAIRoute>(route);
        //}






    }
}
