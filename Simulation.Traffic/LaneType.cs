using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Traffic
{
    public enum LaneType
    {
        Unknown = 0,
        Emergency = 1,
        Train = 2,
        Road = 3,
        Bicycle = 4,
        Path = 5,
        Pedestrian = 6,
        DirtTrack = 7,
        Buslane = 8,
        Highway = 9, 
        Tram = 10
    }
}
