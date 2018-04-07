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

    public struct PathOffsets
    {
        public PathOffsets(float sideOffsetStart, float sideOffsetEnd, float pathOffsetStart, float pathOffsetEnd) : this()
        {
            SideOffsetStart = sideOffsetStart;
            SideOffsetEnd = sideOffsetEnd;
            PathOffsetStart = pathOffsetStart;
            PathOffsetEnd = pathOffsetEnd;
        }

        public float SideOffsetStart { get; }
        public float SideOffsetEnd { get; }

        public float PathOffsetStart { get; }
        public float PathOffsetEnd { get; }
    }

    public interface IAIPath : IAIGraphNode
    {
        IObservableValue<ILoftPath> LoftPath { get; }
        IObservableValue<PathOffsets> Offsets { get; }
 

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