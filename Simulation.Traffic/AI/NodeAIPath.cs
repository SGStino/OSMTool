using System;
using System.Collections.Generic;
using Simulation.Traffic.Lofts;

namespace Simulation.Traffic.AI
{
    public class NodeAIPath : IAIPath, IDisposable
    {
        private readonly SegmentAIPath source;
        private readonly SegmentAIPath destination;
        private readonly ILoftPath path;

        public NodeAIPath(SegmentAIPath source, SegmentAIPath destination, ILoftPath path)
        {
            this.source = source;
            this.destination = destination;
            this.path = path;
        }



        public ILoftPath LoftPath => Path;

        public float SideOffsetStart => 0;

        public float SideOffsetEnd => 0;

        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => false;

        public float MaxSpeed => destination.MaxSpeed;

        public float AverageSpeed => (source.AverageSpeed + destination.AverageSpeed) / 2;

        public IEnumerable<IAIPath> NextPaths => new[] { destination };

        public LaneType LaneType => destination.LaneType;

        public VehicleTypes VehicleTypes => destination.VehicleTypes;

        public float PathOffsetStart => 0.0f;

        public float PathOffsetEnd => 0.0f;

        public ILoftPath Path => path;

        public SegmentAIPath Source => source;

          IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextPaths;

        public void Dispose()
        {
        }
    }
}
