using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using System.Threading;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public class NodeAIPathsFactory : INodeAIPathFactory
    {
        public static NodeAIPathsFactory Default { get; } = new NodeAIPathsFactory();

        public   NodeAIPath[] Create(AINode node)
        {
            var nodeRadius = 10;

            var segments = node.Segments.Select(t => t.Segment).OfType<AISegment>().Where(t => t.Start != null && t.End != null).ToArray();


            var nodePaths = new List<NodeAIPath>();
            for (int iA = 1; iA < segments.Length; iA++)
            {
                for (int iB = 0; iB < iA; iB++)
                {
                    var segA = segments[iA];
                    var loftA = segA.LoftPath;
                    var segB = segments[iB];
                    var loftB = segB.LoftPath;


                    var isAStart = segA.End?.Node == node;
                    var isBStart = segB.Start?.Node == node;

                    var offsetA = isAStart ? nodeRadius : loftA.Length - nodeRadius;
                    var offsetB = isBStart ? nodeRadius : loftB.Length - nodeRadius;

                    var headingA = isAStart ? segA.Start.GetHeading() : segB.End.GetHeading();
                    var headingB = isBStart ? segB.Start.GetHeading() : segB.End.GetHeading();

                    var pointA = loftA.GetTransformedPoint(offsetA, Vector3.zero);
                    var pointB = loftB.GetTransformedPoint(offsetB, Vector3.zero);

                    var loft = new BiArcLoftPath(pointA, headingA, pointB, headingB);

                    //nodePaths.Add()
                }
            }

            return nodePaths.ToArray();
        }
    }

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
            source.ConnectTo(this);
        }


        public ILoftPath Path => path;

        public float PathOffsetStart => source.PathOffsetEnd;

        public float PathOffsetEnd => destination.PathOffsetStart;

        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => false;

        public float MaxSpeed => destination.MaxSpeed;

        public float AverageSpeed => (source.AverageSpeed + destination.AverageSpeed) / 2;

        public IEnumerable<IAIPath> EndConnections => new[] { destination };

        public LaneType LaneType => destination.LaneType;

        public VehicleTypes VehicleTypes => destination.VehicleTypes;


        public void Dispose()
        {
            source.DisconnectFrom(this);
        }
    }
}
