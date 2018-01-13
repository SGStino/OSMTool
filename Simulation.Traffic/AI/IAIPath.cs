using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI
{
    public interface IAIPath
    {
        ILoftPath Path { get; }
        float SideOffsetStart { get; }
        float SideOffsetEnd { get; }

        float PathOffsetStart { get; }
        float PathOffsetEnd { get; }

        IAIPath LeftParralel { get; }
        IAIPath RightParralel { get; }


        bool Reverse { get; }
        float MaxSpeed { get; }
        float AverageSpeed { get; }
        IEnumerable<IAIPath> EndConnections { get; }

        LaneType LaneType { get; }
        VehicleTypes VehicleTypes { get; }
    }

}