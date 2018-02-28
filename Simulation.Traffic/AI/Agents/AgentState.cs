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
        public AgentState(int pathIndex, float progress, IAgentPointer pointer) : this()
        {
            PathIndex = pathIndex;
            Progress = progress;
            Pointer = pointer;
        }
        public IAgentPointer Pointer { get; }
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
        public static AgentStateResult Update(this AgentState currentState, IAgent agent, float dt, GetSpeedDelegate getSpeed, IList<PathDescription> paths, out AgentState newState)
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

            var pointer = currentState.Pointer;

            if (progress >= length)
            {
                do
                { 
                    progress -= length;
                    result |= AgentStateResult.ChangedPath;
                    currentIndex++;
                    if (currentIndex >= paths.Count)
                    {
                        newState = new AgentState(currentIndex, progress, null);
                        return result | AgentStateResult.ReachedEnd;
                    }
                    currentPath = paths[currentIndex];
                    progress += currentPath.Start * currentPath.Path.GetLength(); // move to path start according to description
                    length = currentPath.Path.GetLength() * currentPath.End;
                    currentState.Pointer?.Disconnect();
                }
                while (progress >= length);
                pointer = (currentPath.Path as IAgentChainAIPath)?.Connect(agent);
            }

            newState = new AgentState(currentIndex, progress, pointer);
            return result;
        }
    }
}
