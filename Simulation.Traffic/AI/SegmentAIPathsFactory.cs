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

        public SegmentAIRoute[] CreateRoutes(AISegment segment)
        {
            var lanes = segment.Description.Lanes;
            var paths = new SegmentAIPath[lanes.Length];

            float offset = -lanes.Sum(l => l.Width) / 2;

            for (byte i = 0; i < paths.Length; i++)
            {
                var path = Create(segment, lanes[i], ref offset, i);
                if (i > 0)
                    SegmentAIPath.ConnectParralel(paths[i - 1], path);
                paths[i] = path;
            }

            return paths.GroupBy(t => t.Reverse).Select(t => new SegmentAIRoute(segment, t.ToArray(), t.Key)).ToArray();
        }


        private SegmentAIPath Create(AISegment segment, LaneDescription laneDescription, ref float offset, byte id)
        {
            var path = new SegmentAIPath(segment, laneDescription, offset + laneDescription.Width / 2, id);
            offset += path.Lane.Width;
            return path;
        }
    }

    public static class SegmentAIRouteExtensions
    {
        public static AISegmentNodeConnection GetStart(this SegmentAIRoute route) => route.Reverse ? route.Segment.End : route.Segment.Start;

        public static AISegmentNodeConnection GetEnd(this SegmentAIRoute route) => route.Reverse ? route.Segment.Start : route.Segment.End;
    }

    public class SegmentAIRoute : IAIRoute
    {
        public AISegment Segment { get; }
        public SegmentAIPath[] Paths { get; }

        public bool Reverse { get; }

        IAIPath[] IAIRoute.Paths => Paths;

        public IEnumerable<IAIRoute> NextRoutes => GetRoutes(Reverse ? Segment.End : Segment.Start);

        private IEnumerable<IAIRoute> GetRoutes(AISegmentNodeConnection segmentNodeConnection) => segmentNodeConnection.AIRoutes;

        public float Length => Segment.LoftPath.Length;

        public float Speed => Paths.Sum(t => t.AverageSpeed / Paths.Length);

        public float Cost => 1;

        public SegmentAIRoute(AISegment segment, SegmentAIPath[] segmentAIPath, bool reverse)
        {

            this.Segment = segment;
            this.Paths = segmentAIPath;
            this.Reverse = reverse;
        }


    }

    public interface IAIRoute
    {
        float Length { get; }
        float Speed { get; }
        float Cost { get; }
        IAIPath[] Paths { get; }
        IEnumerable<IAIRoute> NextRoutes { get; }
    }

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
