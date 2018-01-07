using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Simulation.Traffic.AI
{
    public class SegmentAIPathsFactory : ISegmentAIPathsFactory
    {
        public static SegmentAIPathsFactory Default { get; } = new SegmentAIPathsFactory();

        public Task<SegmentAIPath[]> CreateAsync(Segment segment, ILoftPath loftPath, CancellationToken cancel)
        {
            var lanes = segment.Description.Lanes;
            var paths = new SegmentAIPath[lanes.Length];

            float offset = -lanes.Sum(l => l.Width) / 2;

            for (int i = 0; i < paths.Length; i++)
            {
                cancel.ThrowIfCancellationRequested();
                var path = Create(lanes[i], loftPath, ref offset);
                if (i > 0)
                    SegmentAIPath.ConnectParralel(paths[i - 1], path);
                paths[i] = path;
            }

            return Task.FromResult(paths);
        }


        private SegmentAIPath Create(LaneDescription laneDescription, ILoftPath loftPath, ref float offset)
        {
            var path = new SegmentAIPath(laneDescription, loftPath, offset);

            offset += path.Lane.Width;
            return path;
        }
    }



    public class SegmentAIPath : IAIPath, IDisposable
    {
        private LaneDescription laneDescription;
        private readonly ILoftPath loftPath;
        private readonly float offset;
        private List<NodeAIPath> endConnections = new List<NodeAIPath>();
        public SegmentAIPath(LaneDescription laneDescription, ILoftPath loftPath, float offset)
        {
            this.laneDescription = laneDescription;
            this.loftPath = loftPath;
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

        public ILoftPath Path => loftPath;

        public float PathOffsetStart => offset;

        public float PathOffsetEnd => offset;

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
