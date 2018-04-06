using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public class SegmentAIRoute : IAIRoute
    {
        public Segment Segment { get; }
        public SegmentAIPath[] Paths { get; }

        public bool Reverse { get; }

        IReadOnlyList<IAIPath> IAIRoute.Paths => Paths;

        public IEnumerable<NodeAIRoute> NextRoutes => GetRoutes(this.GetEnd());

        private IEnumerable<NodeAIRoute> GetRoutes(ISegmentNodeConnection segmentNodeConnection) => segmentNodeConnection.OutgoingAIRoutes.Value.OfType<NodeAIRoute>();

        public float Length => Segment.LoftPath.Value?.Length ?? 0;

        public float Speed => Paths
            .Select(t => t.AverageSpeed)
            .DefaultIfEmpty(1)
            .Average();

        public float Cost => 1;

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextRoutes;


        public Vector3 StartPosition => this.GetStart()?.GetPosition() ?? Vector3.zero;

        public Vector3 EndPosition => this.GetEnd()?.GetPosition() ?? Vector3.zero;

        IEnumerable<IAIRoute> IAIRoute.NextRoutes => NextRoutes;

        public SegmentAIRoute(Segment segment, SegmentAIPath[] segmentAIPath, bool reverse)
        {

            this.Segment = segment;
            this.Paths = segmentAIPath;
            this.Reverse = reverse;
        }


    }
}
