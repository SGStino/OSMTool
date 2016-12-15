using Simulation.Traffic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSMTool.Wpf.Traffic
{
    public class TrafficSegmentDescription : SegmentDescription
    {
        public int LaneCount { get; set; } = 1;
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsOneWay { get; set; }
        public bool IsRoundabout { get; internal set; }

        public OsmSharp.Way OsmWay { get; set; }

        internal static bool CanMerge(TrafficSegmentDescription a, TrafficSegmentDescription b)
        {
            return 
                a.LaneCount == b.LaneCount && 
                a.IsOneWay == b.IsOneWay && 
                a.IsRoundabout == b.IsRoundabout && 
                a.Type == b.Type;
        }
    }
}
