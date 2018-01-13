using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;

namespace Simulation.Traffic.AI
{
    public class SegmentAIPath : IAIPath, IDisposable
    {
        public byte ID { get; }
        private readonly AISegment segment;
        private LaneDescription laneDescription;
        private readonly float offset;
        private List<NodeAIPath> endConnections = new List<NodeAIPath>();
        public SegmentAIPath(AISegment segment, LaneDescription laneDescription, float offset, byte id)
        {
            this.segment = segment;
            this.laneDescription = laneDescription;
            this.offset = offset;
        }

        public void ConnectTo(NodeAIPath path)
        {
            endConnections.Add(path);
        }
        public void DisconnectFrom(NodeAIPath path)
        {
            endConnections.Remove(path);
        }

        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => laneDescription.Reverse;

        public IEnumerable<IAIPath> EndConnections => endConnections;

        public LaneType LaneType => laneDescription.LaneType;

        public VehicleTypes VehicleTypes => laneDescription.VehicleTypes;

        public float MaxSpeed => laneDescription.MaxSpeed;

        public float AverageSpeed => laneDescription.MaxSpeed; // TODO: make variable with vehicles

        public LaneDescription Lane => laneDescription;

        public ILoftPath Path => segment.LoftPath;

        public float SideOffsetStart => offset;

        public float SideOffsetEnd => offset;

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public static void ConnectParralel(SegmentAIPath left, SegmentAIPath right)
        {
            left.RightParralel = right;
            right.LeftParralel = left;
        }

        public void Dispose()
        {
            endConnections.Clear();
        }
    }
}
