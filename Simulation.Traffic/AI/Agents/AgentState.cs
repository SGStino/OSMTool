using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.AI.Agents
{
    public struct AgentState
    {
        public AgentState(int pathIndex, float progress) : this()
        {
            PathIndex = pathIndex;
            Progress = progress;
        }
        public int PathIndex { get; }
        public float Progress { get; }
    }


    public delegate bool GetNextPathDelegate(AgentState state, int index, out PathDescription path);
    public delegate float GetSpeedDelegate(AgentState state);

    public enum AgentStateResult
    {
        None = 0,
        Moved = 1,
        ChangedPath = 2,
        ReachedEnd = 4
    }

    public static class AgentStateExtensions
    {
        // TODO: disconnnect agent from it's state update
        public static AgentStateResult Update(this AgentState currentState, float dt, GetSpeedDelegate getSpeed, IList<PathDescription> paths, out AgentState newState)
        {
            var result = AgentStateResult.None;
            var speed = getSpeed(currentState);

            var progress = currentState.Progress;

            var increment = speed * dt;
            progress += increment;

            if (Mathf.Abs(increment) > 0.000001)
                result |= AgentStateResult.Moved;

            var currentIndex = currentState.PathIndex;

            var currentPath = paths[currentIndex];

            var length = currentPath.Path.GetLength() * currentPath.End;


            if (progress >= length)
            {
                do
                {
                    progress -= length;
                    result |= AgentStateResult.ChangedPath;
                    currentIndex++;
                    if (currentIndex >= paths.Count)
                    {
                        newState = new AgentState(currentIndex, progress);
                        return result | AgentStateResult.ReachedEnd;
                    }
                    currentPath = paths[currentIndex];
                    progress += currentPath.Start * currentPath.Path.GetLength(); // move to path start according to description
                    length = currentPath.Path.GetLength() * currentPath.End;
                }
                while (progress >= length);

            }

            newState = new AgentState(currentIndex, progress);
            return result;
        }

        public static float DistanceToNextAgent(this AgentState state, IAgentPointer pointer, float searchDistance, IList<PathDescription> pathSequence, out AgentState nextAgentState)
        {

            var nextAgent = pointer?.Next;

            if (nextAgent != null)
            {
                //if (dist < 0) throw new InvalidOperationException("the next agent is behind us!");
                nextAgentState = new AgentState(state.PathIndex, nextAgent.Agent.Progress);
                return (nextAgent.Agent.Progress - state.Progress) - nextAgent.Agent.Length;
            }

            var paths = pathSequence;
            var currentPath = paths[state.PathIndex];

            var end = currentPath.Path.GetLength() * currentPath.End;

            var distanceToEnd = end - state.Progress;


            var offset = distanceToEnd;

            for (int i = state.PathIndex + 1; i < pathSequence.Count; i++)
            {
                var path = pathSequence[i];
                nextAgent = (path.Path as IAgentChainAIPath)?.Agents.Last;
                if (nextAgent == null)
                {
                    offset = path.Path.GetLength() * (path.End - path.Start);
                }
                else
                {
                    var progress = nextAgent.Agent.Progress - (path.Path.GetLength() * path.Start);
                    nextAgentState = new AgentState(i, nextAgent.Agent.Progress);
                    return progress + offset - nextAgent.Agent.Length;
                }
                if (offset > searchDistance) // stop searching ahead for to far.
                    break;
            }
            nextAgentState = new AgentState(pathSequence.Count, 0);
            return float.PositiveInfinity; // no next agent found

        }

    }
}
