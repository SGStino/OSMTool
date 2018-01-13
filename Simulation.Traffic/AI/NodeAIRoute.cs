using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public class NodeAIRoute : IAIRoute, IDisposable
    {
        private AISegmentNodeConnection con;
        private readonly SegmentAIRoute target;
        private readonly AISegmentNodeConnection con2;

        public NodeAIRoute(AISegmentNodeConnection con, SegmentAIRoute target, NodeAIPath[] paths)
        {
            this.con = con;
            this.target = target;
            Paths = paths;

            con2 = target.GetStart();
        }

        public AINode Node => con.Node;

        public float Length => 1;

        public float Speed => 1;

        public float Cost => 1;

        public NodeAIPath[] Paths { get; }


        public IEnumerable<IAIRoute> NextRoutes => new[] { target };

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextRoutes;

        IAIPath[] IAIRoute.Paths => Paths;


        public Vector3 StartPosition => con.GetPosition();

        public Vector3 EndPosition => con2.GetPosition();

        public void Dispose()
        {
        }
    }
}
