using Simulation.Traffic.AI.Navigation;
using Simulation.Traffic.Lofts;
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



    [Flags]
    public enum AgentJobState
    {
        FindingRoute = 1 | Running,
        FindingPath = 2 | Running,
        Completed = 4,
        Running = 8,
        Error = 16,
        RouteNotFound = 32 | Error,
        PathNotFound = 64 | Error,
        Cancelled = 128 | Error
    }

    public static class AgentStateExtensions
    {
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

        private AgentJobState currentState;

        public void Start()
        {
            StartRouteFinding();
        }


        private Sequence<IAIPath> pathSequence;
        private Sequence<IAIRoute> routeSequence;

        public event Action<AgentJob, AgentJobState> StateChanged;

        private void SetState(AgentJobState state)
        {
            currentState = state;
            StateChanged?.Invoke(this, state);
        }

        public AgentJobState CurrentState => currentState;

        public void Iterate()
        {
            switch (currentState)
            {
                case AgentJobState.FindingRoute:
                    if (!routeSolver.Iterate())
                        CompleteRouteFinding();
                    break;
                case AgentJobState.FindingPath:
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
                SetState(AgentJobState.Completed);
            }
            else
            {
                SetState(AgentJobState.PathNotFound);
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
                SetState(AgentJobState.RouteNotFound);
            }
            routeSolver = null;
        }

        private void StartRouteFinding()
        {
            if (endCriteria.Routes?.Any() ?? false)
            {
                routeSolver = new RouteSolver(startCriteria.Routes, endCriteria.Routes);
                SetState(AgentJobState.FindingRoute);
            }
            else
            {
                SetState(AgentJobState.RouteNotFound);
            }
        }
        private void StartPathFinding()
        {
            var sourcePaths = GetSourcePaths();
            var destinationPaths = GetDestinationPaths();

            if (destinationPaths?.Any() ?? false)
            {
                pathSolver = new PathSolver(sourcePaths, destinationPaths, routeSequence);

                SetState(AgentJobState.FindingPath);
            }
            else
            {
                SetState(AgentJobState.PathNotFound);
            }
        }

        protected virtual IEnumerable<IAIPath> GetDestinationPaths() =>
            routeSequence?.Any() ?? false
            ? endCriteria.Paths != null
                ? endCriteria.Paths.Intersect(routeSequence.Last().Paths)
                : routeSequence.LastOrDefault()?.Paths
            : null;


        protected virtual IEnumerable<IAIPath> GetSourcePaths() =>
            routeSequence?.Any() ?? false
            ? startCriteria?.Paths != null
                    ? routeSequence.CurrentItem.Paths.Intersect(startCriteria.Paths)
                    : routeSequence.CurrentItem.Paths
            : null;

        public void Dispose()
        {
            SetState(AgentJobState.Cancelled);
        }
    }
    [Flags]
    public enum AgentState
    {
        Initializing = 1,
        WaitingForRoute = 2,
        NoRouteFound = 4,
        GoingToRoute = 8,
        FollowingRoute = 16,
        GoingToDestination = 32,
        DestinationReached = 64,
    }

    public class Agent
    {
        private AgentState state = AgentState.Initializing;
        private Sequence<IAIRoute> routeSequence;
        private Sequence<IAIPath> pathSequence;

        // going from source to first path
        private IAIPath approachPath;
        // point to arrive at on the first path
        private float approachProgress;
        // going from last path to destination
        private IAIPath departPath;
        // point to depart from the last path
        private float departProgress;

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
        public IEnumerable<IAIRoute> RouteSequence => routeSequence;
        public IEnumerable<IAIPath> PathSequence => pathSequence;
        public IAIRoute CurrentRoute => routeSequence?.CurrentItem;
        public IAIPath CurrentPath => state == AgentState.GoingToRoute ?  approachPath : (state == AgentState.GoingToDestination) ? departPath : pathSequence?.CurrentItem;
        public AgentState CurrentState => state;
        public void Update(float dt)
        {
            switch (state)
            {
                case AgentState.WaitingForRoute:
                    {
                        if (solverJob.CurrentState.HasFlag(AgentJobState.Running))
                            return; // still waiting, agent is done for this cyclus
                        if (solverJob.CurrentState.HasFlag(AgentJobState.Completed))
                        {
                            updateApproachRoute();
                            updateDepartRoute();
                            state = AgentState.GoingToRoute;
                            goto case AgentState.GoingToRoute;
                        }
                        if (solverJob.CurrentState.HasFlag(AgentJobState.Error))
                        {
                            state = AgentState.NoRouteFound;
                            goto case AgentState.NoRouteFound;
                        }
                    }
                    break;
                case AgentState.GoingToRoute:
                    {
                        progress += dt * (CurrentPath?.AverageSpeed ?? 1);
                        isLastKnownTransformValid = false;
                        var pathLength = approachPath.GetLength();
                        if (progress > pathLength)
                        {
                            progress -= pathLength;
                            progress += approachProgress;
                            state = AgentState.FollowingRoute;
                        }
                    }
                    break;
                case AgentState.FollowingRoute:
                    {
                        progress += dt * (CurrentPath?.AverageSpeed ?? 1);
                        isLastKnownTransformValid = false;
                        var pathLength = pathSequence.IsLast() ? departProgress : pathSequence.CurrentItem.GetLength();
                        if (progress > pathLength)
                        {
                            progress -= pathLength;
                            if (!pathSequence.Next())
                            {
                                state = AgentState.GoingToDestination;
                                pathSequence = null;
                            }
                        }
                    }
                    break;
                case AgentState.GoingToDestination:
                    {
                        progress += dt * (CurrentPath?.AverageSpeed ?? 1);
                        isLastKnownTransformValid = false;
                        var pathLength = departPath.GetLength();
                        if (progress > pathLength)
                        {
                            progress -= pathLength;
                            state = AgentState.DestinationReached;
                            DestinationReached();
                        }
                    }
                    break;
                case AgentState.NoRouteFound:
                    {
                        // agent is lost! find a way to get out of this situation.
                    }
                    break;
            }
        }

        // todo: make smarter, should use driveways of buildings instead of void paths
        private void updateDepartRoute()
        {
            Vector3 start, startTangent, end, endTangent;

            var path = pathSequence[pathSequence.Count - 1];

            path.LoftPath.SnapTo(destination, out start, out var distance);

            departProgress = path.GetDistanceFromLoftPath(distance);
            end = destination;

            startTangent = -path.GetTransform(departProgress).MultiplyVector(Vector3.forward);

            endTangent = (start - end).normalized;

            var loft = new BiArcLoftPath(start, startTangent, end, endTangent);

            departPath = new AgentAIPath(loft);
        }
        // todo: make smarter, should use driveways of buildings of void paths
        private void updateApproachRoute()
        {
            Vector3 start, startTangent, end, endTangent;

            var startTransform = CurrentTransform;

            start = startTransform.MultiplyPoint3x4(Vector3.zero);
            startTangent = startTransform.MultiplyVector(Vector3.forward);

            var path = pathSequence[0];

            path.LoftPath.SnapTo(start, out end, out var distance);
            this.approachProgress = path.GetDistanceFromLoftPath(distance);


            var transform = path.GetTransform(approachProgress);

            endTangent = -transform.MultiplyVector(Vector3.forward);


            var loft = new BiArcLoftPath(start, startTangent, end, endTangent);

            approachPath = new AgentAIPath(loft);
        }

        protected virtual void DestinationReached()
        {
            state = AgentState.DestinationReached;
        }

        private float progress;

        private Matrix4x4 lastKnownTransform;
        private bool isLastKnownTransformValid;
        private Vector3 destination;

        public Matrix4x4 CurrentTransform => isLastKnownTransformValid ? lastKnownTransform : (lastKnownTransform = getTransform());//pathSequence?.CurrentItem.LoftPath.GetTransform(progress, getBaseTransform(progress));

        private Matrix4x4 getTransform()
        {
            switch (state)
            {
                case AgentState.FollowingRoute:
                    return CurrentPath.GetTransform(progress);
                case AgentState.GoingToRoute:
                    return approachPath.GetTransform(progress);
                case AgentState.GoingToDestination:
                    return departPath.GetTransform(progress);
                case AgentState.DestinationReached:
                    return departPath.GetEndTransform();
            }
            return Matrix4x4.zero;
        }

        public void Teleport(Matrix4x4 start)
        {
            lastKnownTransform = start;
            isLastKnownTransformValid = true;
            var startPosition = start.GetTranslate();
            if (finder.Find(startPosition, out var routes, out var paths))
            {
                solverJob.SetSource(routes, paths);
                ActivateJob();
            }
            else
            {
                state = AgentState.NoRouteFound;
            }
        }

        private void ActivateJob()
        {
            solverJob.Start();
            runner.Run(solverJob);
            state = AgentState.WaitingForRoute;
        }

        public void SetDestination(Vector3 position)
        {
            destination = position;
            if (finder.Find(position, out var routes, out var paths))
            {
                solverJob.SetDestination(routes, paths);
                ActivateJob();
            }
            else
            {
                state = AgentState.NoRouteFound;
            }
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

    internal class AgentAIPath : IAIPath
    {

        public AgentAIPath(BiArcLoftPath loft)
        {
            this.LoftPath = loft;
        }

        public ILoftPath LoftPath { get; }

        public float SideOffsetStart => 0;

        public float SideOffsetEnd => 0;

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public IAIPath LeftParralel => null;

        public IAIPath RightParralel => null;

        public bool Reverse => false;

        public float MaxSpeed => 50; // TODO max speed

        public float AverageSpeed => MaxSpeed;

        public IEnumerable<IAIPath> NextPaths => Enumerable.Empty<IAIPath>(); // not used

        public LaneType LaneType => LaneType.DirtTrack;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths;
    }
}
