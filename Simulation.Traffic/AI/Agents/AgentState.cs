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
        public AgentState(PathDescription currentPath, float progress, IAgentPointer pointer) : this()
        {
            Path = currentPath;
            Progress = progress;
            Pointer = pointer;
        }
        public IAgentPointer Pointer { get; }
        public PathDescription Path { get; }
        public float Progress { get; }
    }


    public delegate bool GetNextPathDelegate(AgentState state, out PathDescription path);
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
        public static AgentStateResult Update(this AgentState currentState, IAgent agent, float dt, GetSpeedDelegate getSpeed, GetNextPathDelegate nextPath, out AgentState newState)
        {
            var result = AgentStateResult.None;
            var speed = getSpeed(currentState);

            var progress = currentState.Progress;

            var increment = speed * dt;
            progress += increment;

            if (Mathf.Abs(increment) > 0.000001)
                result |= AgentStateResult.Moved;


            var currentPath = currentState.Path;

            var length = currentPath.Path.GetLength() * currentPath.End;

            var pointer = currentState.Pointer;

            if (progress> length)
            {
                do
                {
                    progress -= length;
                    result |= AgentStateResult.ChangedPath;
                    if (!nextPath(currentState, out currentPath))
                    {
                        newState = new AgentState(currentPath, progress, null);
                        return result | AgentStateResult.ReachedEnd;
                    }
                    progress += currentPath.Start * currentPath.Path.GetLength(); // move to path start according to description
                    currentState.Pointer?.Disconnect();
                }
                while (progress > length);
                pointer = (currentPath.Path as IAgentChainAIPath)?.Connect(agent);
            }

            newState = new AgentState(currentPath, progress, pointer);
            return result;
        }
    }
}
