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

    public class Agent : IAgent, IDisposable
    {
        public float SpeedVariance { get; set; } = 1.0f;
        private AgentStatus status = AgentStatus.Initializing;
        private IList<PathDescription> pathSequence;
        private AgentState currentState;
        private AgentState previousState;



        private readonly AgentJob solverJob;
        private readonly IAIRouteFinder finder;
        private readonly IAgentJobRunner runner;


        public Agent(IAIRouteFinder finder, IAgentJobRunner runner)
        {
            solverJob = new AgentJob();
            this.finder = finder;
            this.runner = runner;
        }


        private IAIPath getPath(AgentState state) => state.PathIndex >= 0 && state.PathIndex < pathSequence.Count ? pathSequence[state.PathIndex].Path : null;
        public IAIPath CurrentPath => getPath(currentState);
        public AgentStatus CurrentStatus => status;

        public IList<PathDescription> CurrentSequence => pathSequence;
        public AgentState CurrentState => currentState;
        public AgentState PreviousState => previousState;
        private float getSpeed(AgentState state) => getSpeed(state, MaxSpeed);

        public void Update(float dt)
        {
            AgentState newState = currentState;
            switch (status)
            {
                case AgentStatus.WaitingForRoute:
                    {
                        if (solverJob.CurrentState.HasFlag(AgentJobStatus.Running))
                            return; // still waiting, agent is done for this cyclus
                        if (solverJob.CurrentState.HasFlag(AgentJobStatus.Completed))
                        {
                            var path = solverJob.PathSequence;

                            var startTransform = CurrentTransform;



                            this.pathSequence = solverJob.PathSequence.Extend(startTransform.GetTranslate(), destination);
                             

                            //                            throw new InvalidOperationException("Something goes wrong here, approach and end routes should be somewhere halfway the segments they are attached to");


                            currentState = createInitialState(0);
                            status = AgentStatus.GoingToRoute;
                            goto case AgentStatus.GoingToRoute;
                        }
                        if (solverJob.CurrentState.HasFlag(AgentJobStatus.Error))
                        {
                            status = AgentStatus.NoRouteFound;
                            goto case AgentStatus.NoRouteFound;
                        }
                    }
                    break;
                case AgentStatus.GoingToRoute:
                    {
                        var result = currentState.Update(this, dt, getSpeed, pathSequence, out newState);
                        if (result.HasFlag(AgentStateResult.ChangedPath))
                            status = AgentStatus.FollowingRoute;
                        if (result.HasFlag(AgentStateResult.Moved))
                            isLastKnownTransformValid = false;
                    }
                    break;
                case AgentStatus.FollowingRoute:
                    {
                        var result = currentState.Update(this, dt, getSpeed, pathSequence, out newState);
                        if (result.HasFlag(AgentStateResult.ChangedPath))
                        {
                            if (newState.PathIndex == pathSequence.Count - 1)
                                status = AgentStatus.GoingToDestination;
                        }
                        if (result.HasFlag(AgentStateResult.Moved))
                            isLastKnownTransformValid = false;
                    }
                    break;
                case AgentStatus.GoingToDestination:
                    {
                        var result = currentState.Update(this, dt, getSpeed, pathSequence, out newState);
                        if (result.HasFlag(AgentStateResult.ChangedPath))
                            DestinationReached();
                        if (result.HasFlag(AgentStateResult.Moved))
                            isLastKnownTransformValid = false;
                    }
                    break;
                case AgentStatus.NoRouteFound:
                    {
                        // agent is lost! find a way to get out of this situation.
                    }
                    break;
            }
            previousState = currentState;
            currentState = newState;
        }

        private AgentState createInitialState(int index) => new AgentState(index, 0, (pathSequence[index].Path as IAgentChainAIPath)?.Connect(this));


        private bool getNextPath(AgentState state, int index, out PathDescription path)
        {
            if (index < pathSequence.Count)
            {
                path = pathSequence[index];
                return true;
            }
            path = default(PathDescription);
            return false;
        }





        public float MaxSpeed { get; set; } = float.PositiveInfinity;

        private float safetyDistance = 10;
        private float speedDistanceFactor = 20;
        private float getSpeed(AgentState state, float maxSpeed)
        {
            var desiredSpeed = pathSequence[state.PathIndex].Path.MaxSpeed;

            var agentPointer = state.Pointer;
            maxSpeed = Math.Min(desiredSpeed, maxSpeed);
            var next = agentPointer?.Next;
            if (next != null)
            {
                var dist = next.Agent.Progress - this.Progress;

                var p = (dist - safetyDistance) / speedDistanceFactor;
                if (p < 0) // stand still, will you
                    return 0;
                if (p < 1)
                    return Mathf.Sqrt(p) * maxSpeed;
                return maxSpeed * SpeedVariance;
            }
            return maxSpeed * SpeedVariance;
        }

        public float CurrentSpeed { get; private set; }

        // todo: make smarter, should use driveways of buildings instead of void paths
        private void updateDepartRoute(Sequence<PathDescription> pathSequence)
        {
            throw new NotImplementedException();
            //    Vector3 start, startTangent, end, endTangent;


            //    var index = pathSequence.Count - 1;
            //    var pathDesc = pathSequence[index];
            //    var path = pathDesc.Path;

            //    path.LoftPath.SnapTo(Destination, out var distance);

            //    var start = path.

            //    var departProgress = path.GetDistanceFromLoftPath(distance);

            //    var endProgress = departProgress / path.GetLength();

            //    pathSequence[index] = new PathDescription(path, 0, endProgress);

            //    end = Destination;

            //    startTangent = -path.GetTransform(departProgress).MultiplyVector(Vector3.forward);

            //    endTangent = (start - end).normalized;

            //    var loft = new BiArcLoftPath(start, startTangent, end, endTangent);

            //    var departPath = new AgentAIPath(loft);

            //    var desc = new PathDescription(departPath, 0, 1);
            //    pathSequence.Add(desc);
        }
        // todo: make smarter, should use driveways of buildings of void paths
        private void updateApproachRoute(Sequence<PathDescription> pathSequence)
        {
            throw new NotImplementedException();
            //Vector3 start, startTangent, end, endTangent;

            //var startTransform = CurrentTransform;



            //start = startTransform.MultiplyPoint3x4(Vector3.zero);
            //startTangent = startTransform.MultiplyVector(Vector3.forward);

            //var pathDesc = pathSequence[0];
            //var path = pathDesc.Path;

            //path.LoftPath.SnapTo(start, out end, out var distance);
            //var approachProgress = path.GetDistanceFromLoftPath(distance);


            //var transform = path.GetTransform(approachProgress);

            //pathSequence[0] = new PathDescription(path, approachProgress / path.GetLength(), 1);

            //endTangent = -transform.MultiplyVector(Vector3.forward);


            //var loft = new BiArcLoftPath(start, startTangent, end, endTangent);

            //var approachPath = new AgentAIPath(loft);

            //var descr = new PathDescription(Path: approachPath, Start: 0, End: 1);

            //pathSequence.Insert(0, descr);
        }

        protected virtual void DestinationReached()
        {
            currentState.Pointer?.Disconnect();
            status = AgentStatus.DestinationReached;
        }


        private Matrix4x4 lastKnownTransform;
        private bool isLastKnownTransformValid;
        private Vector3 startPosition;
        private Vector3 destination;


        public Matrix4x4 CurrentTransform => isLastKnownTransformValid ? lastKnownTransform : (lastKnownTransform = getTransform(currentState));//pathSequence?.CurrentItem.LoftPath.GetTransform(progress, getBaseTransform(progress));

        public float Progress => currentState.Progress;

        public Vector3 StartPosition { get => startPosition; }
        public Vector3 Destination { get => destination; }

        private Matrix4x4 getTransform(AgentState state) => getPath(state)?.GetTransform(state.Progress) ?? Matrix4x4.zero;



        public void Teleport(Matrix4x4 start)
        {
            lastKnownTransform = start;
            isLastKnownTransformValid = true;
            startPosition = start.GetTranslate();
            if (finder.Find(StartPosition, out var routes, out var paths))
            {
                solverJob.SetSource(routes, paths);
                ActivateJob();
            }
            else
            {
                status = AgentStatus.NoRouteFound;
            }
        }

        private void ActivateJob()
        {
            solverJob.Start();
            runner.Run(solverJob);
            status = AgentStatus.WaitingForRoute;
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
                status = AgentStatus.NoRouteFound;
            }
        }

        public void Dispose()
        {
            DestinationReached();
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
