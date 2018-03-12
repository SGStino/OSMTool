using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public class SegmentAIRoute : IAIRoute
    {
        public AISegment Segment { get; }
        public SegmentAIPath[] Paths { get; }

        public bool Reverse { get; }

        IAIPath[] IAIRoute.Paths => Paths;

        public IEnumerable<NodeAIRoute> NextRoutes => GetRoutes(this.GetEnd());

        private IEnumerable<NodeAIRoute> GetRoutes(AISegmentNodeConnection segmentNodeConnection) => segmentNodeConnection.AIRoutes;

        public float Length => Segment.LoftPath?.Length ?? 0;

        public float Speed => Paths
            .Select(t => t.AverageSpeed)
            .DefaultIfEmpty(1)
            .Average();

        public float Cost => 1;

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextRoutes;


        public Vector3 StartPosition => this.GetStart()?.GetPosition() ?? Vector3.zero;

        public Vector3 EndPosition => this.GetEnd()?.GetPosition() ?? Vector3.zero;

        IEnumerable<IAIRoute> IAIRoute.NextRoutes => NextRoutes;

        public SegmentAIRoute(AISegment segment, SegmentAIPath[] segmentAIPath, bool reverse)
        {

            this.Segment = segment;
            this.Paths = segmentAIPath;
            this.Reverse = reverse;
        }


    }
}
