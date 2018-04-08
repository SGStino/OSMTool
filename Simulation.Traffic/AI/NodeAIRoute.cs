using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Simulation.Traffic.AI
{
    public class NodeAIRoute : IAIRoute, IDisposable
    {
        private ISegmentNodeConnection source;
        private readonly SegmentAIRoute target; 

        public NodeAIRoute(ISegmentNodeConnection source, SegmentAIRoute target, NodeAIPath[] paths)
        {
            this.source = source;
            this.target = target;
            Paths = paths;

            Destination = target.GetStart();         }

        public ISegmentNodeConnection Source { get; }
        public ISegmentNodeConnection Destination { get; }

        public INode Node => source.Node;

        public float Length => 1;

        public float Speed => 1;

        public float Cost => 1;

        public NodeAIPath[] Paths { get; }


        public IEnumerable<IAIRoute> NextRoutes => new[] { target };

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextRoutes;

        IReadOnlyList<IAIPath> IAIRoute.Paths => Paths;


        public Vector3 StartPosition => source.GetPosition();

        public Vector3 EndPosition => Destination.GetPosition();

        public void Dispose()
        {
        }
    }
}
