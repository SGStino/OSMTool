using System;

namespace Simulation.Traffic.AI.Agents
{
    [Flags]
    public enum AgentStatus
    {
        Initializing = 1,
        WaitingForRoute = 2,
        NoRouteFound = 4,
        GoingToRoute = 8,
        FollowingRoute = 16,
        GoingToDestination = 32,
        DestinationReached = 64,
        InternalError = 128,
    }
}
