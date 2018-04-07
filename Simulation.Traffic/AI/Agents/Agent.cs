using Simulation.Data;
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
        public float Length { get; set; } = 5;

        public float SpeedVariance { get; set; } = 1.0f;
        private AgentStatus status = AgentStatus.Initializing;
        private IList<PathDescription> pathSequence;
        private AgentState currentState;
        private AgentState previousState;



        private readonly AgentJob solverJob;
        private readonly IAIRouteFinder finder;
        private readonly IAgentJobRunner runner;


        private IAgentPointer pointer;

        public IAgentPointer Pointer => pointer;
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
                        var result = update(dt, out newState);
                        if (result.HasFlag(AgentStateResult.ChangedPath))
                            status = AgentStatus.FollowingRoute;
                        if (result.HasFlag(AgentStateResult.Moved))
                            isLastKnownTransformValid = false;
                    }
                    break;
                case AgentStatus.FollowingRoute:
                    {
                        var result = update(dt, out newState);
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
                        var result = update(dt, out newState);
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

        private AgentStateResult update(float dt, out AgentState newState)
        {
            var result = currentState.Update(dt, getSpeed, pathSequence, out newState);

            if (result.HasFlag(AgentStateResult.ChangedPath))
            {
                pointer?.Disconnect(); // disconnect existing pointer;
                pointer = null;
                var i = newState.PathIndex;
                if (i < pathSequence.Count)
                {
                    var seg = pathSequence[newState.PathIndex];
                    pointer = (seg.Path as IAgentChainAIPath).Connect(this); // connect new pointer
                }
            }
            return result;
        }

        private AgentState createInitialState(int index) => new AgentState(index, 0);


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

        private float safetyDistance = 6;
        private float speedDistanceFactor = 20;
        private float getSpeed(AgentState state, float maxSpeed)
        {
            var desiredSpeed = pathSequence[state.PathIndex].Path.MaxSpeed;

            // todo search through upcoming tracks too
            var agentPointer = pointer;
            maxSpeed = Math.Min(desiredSpeed, maxSpeed);

            var dist = state.DistanceToNextAgent(pointer, safetyDistance, pathSequence, out var nextAgent);
            if (!float.IsInfinity(dist))
            {
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



        protected virtual void DestinationReached()
        {
            pointer?.Disconnect();
            pointer = null;
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

        public float Speed => getSpeed(currentState);

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
            LoftPath = new BehaviorSubjectValue<ILoftPath>(loft);
            Offsets = new BehaviorSubjectValue<PathOffsets>(new PathOffsets(0, 0, 0, 0));
        }

        public IObservableValue<ILoftPath> LoftPath { get; }
        public IObservableValue<PathOffsets> Offsets { get; }
      

        public IAIPath LeftParralel => null;

        public IAIPath RightParralel => null;

        public bool Reverse => false;

        public float MaxSpeed => 50; // TODO max speed

        public float AverageSpeed => MaxSpeed;

        public IObservableValue<IEnumerable<IAIPath>> NextPaths { get; } = new BehaviorSubjectValue<IEnumerable<IAIPath>>(Enumerable.Empty<IAIPath>()); // not used

        public LaneType LaneType => LaneType.DirtTrack;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths.Value;
    }
}
