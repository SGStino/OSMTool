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

        public IEnumerable<IAIRoute> NextRoutes => GetRoutes(this.GetEnd());

        private IEnumerable<IAIRoute> GetRoutes(AISegmentNodeConnection segmentNodeConnection) => segmentNodeConnection.AIRoutes;

        public float Length => Segment.LoftPath?.Length ?? 0;

        public float Speed => Paths.Sum(t => t.AverageSpeed / Paths.Length);

        public float Cost => 1;

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextRoutes;


        public Vector3 StartPosition => this.GetStart()?.GetPosition() ?? Vector3.zero;

        public Vector3 EndPosition => this.GetEnd()?.GetPosition() ?? Vector3.zero;


        public SegmentAIRoute(AISegment segment, SegmentAIPath[] segmentAIPath, bool reverse)
        {

            this.Segment = segment;
            this.Paths = segmentAIPath;
            this.Reverse = reverse;
        }


    }
}
