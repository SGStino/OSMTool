using Simulation.Data;
using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI
{
    public interface IAIPath : IAIGraphNode
    {
        IObservableValue<ILoftPath> LoftPath { get; }
        float SideOffsetStart { get; }
        float SideOffsetEnd { get; }

        float PathOffsetStart { get; }
        float PathOffsetEnd { get; }

        IAIPath LeftParralel { get; }
        IAIPath RightParralel { get; }


        bool Reverse { get; }
        float MaxSpeed { get; }
        float AverageSpeed { get; }
        IObservableValue<IEnumerable<IAIPath>> NextPaths { get; }

        LaneType LaneType { get; }
        VehicleTypes VehicleTypes { get; }
    }


    public interface IAgentChainAIPath : IAIPath
    {
        IAgentChain Agents { get; }
    }
}