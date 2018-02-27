using System;

namespace Simulation.Traffic.AI.Agents
{
    [Flags]
    public enum AgentJobStatus
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
}
