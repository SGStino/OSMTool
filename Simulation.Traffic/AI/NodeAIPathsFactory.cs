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
    public class NodeAIPathsFactory : INodeAIPathsFactory
    {
        public static NodeAIPathsFactory Default { get; } = new NodeAIPathsFactory();

        public NodeAIPath[] Create(AINode node)
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

        public NodeAIRoute[] CreateRoutes(AISegmentNodeConnection con)
        {
            var incomingRoutes = GetRoutes(con, false);

            var outgoingRoutes = con.Node.Segments.SelectMany(t => GetRoutes(t, true));

            var routes = new List<NodeAIRoute>();
            foreach (var incoming in incomingRoutes)
            {
                foreach (var outgoing in outgoingRoutes)
                {
                    var paths = create(incoming, outgoing);

                    var node = new NodeAIRoute(con, outgoing.Segment, paths.ToArray());

                    routes.Add(node);
                }
            }
            return routes.ToArray();
        }

        private IEnumerable<NodeAIPath> create(SegmentAIRoute incoming, SegmentAIRoute outgoing)
        {
            var from = incoming.GetEnd();
            var to = outgoing.GetStart();
            foreach (var iPath in incoming.Paths)
            {
                foreach (var oPath in outgoing.Paths)
                {
                    if (iPath.Path != null && oPath.Path != null)
                        yield return new NodeAIPath(iPath, oPath, createLineLoft(iPath, from, oPath, to));
                }
            }
        }

        private ILoftPath createLineLoft(SegmentAIPath iPath, AISegmentNodeConnection from, SegmentAIPath oPath, AISegmentNodeConnection to)
        {
            var point1 = from.GetPosition();
            var point2 = to.GetPosition();
            return new BiArcLoftPath(point1, from.Tangent, point2, -to.Tangent);
            return new LinearPath(point1, point2);
        }

        private static Vector3 GetPoint(SegmentAIPath iPath, bool end)
        {
            var point = new Vector3(iPath.Reverse ^ end ? iPath.PathOffsetStart : iPath.PathOffsetEnd, 0, 0);
            var p = iPath.Reverse ^ end ? iPath.Path.Length : 0;
            point = iPath.Path.GetTransformedPoint(p, point);
            return point;
        }

        private IEnumerable<SegmentAIRoute> GetRoutes(AISegmentNodeConnection con, bool incoming)
        {
            var isEnd = con.Segment.End == con;



            return GetRoutes(isEnd ^ incoming, con.Segment);
        }

        private static IEnumerable<SegmentAIRoute> GetRoutes(bool isEnd, AISegment segment)
        {
            return segment?.AIRoutes.Where(t => t.Reverse != isEnd);
        }
    }

    public class NodeAIRoute : IAIRoute, IDisposable
    {
        private AISegmentNodeConnection con;
        private AISegment segment;

        public NodeAIRoute(AISegmentNodeConnection con, AISegment segment, NodeAIPath[] paths)
        {
            this.con = con;
            this.segment = segment;
            Paths = paths;
        }

        public float Length => 1;

        public float Speed => 1;

        public float Cost => 1;

        public NodeAIPath[] Paths { get; }

        public IEnumerable<IAIRoute> NextRoutes => Enumerable.Empty<IAIRoute>();

        IAIPath[] IAIRoute.Paths => Paths;

        public void Dispose()
        {
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

        public float SideOffsetStart => source.Reverse ? -source.SideOffsetStart : source.SideOffsetEnd;

        public float SideOffsetEnd => destination.Reverse ? -destination.SideOffsetEnd : destination.SideOffsetStart;

        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => false;

        public float MaxSpeed => destination.MaxSpeed;

        public float AverageSpeed => (source.AverageSpeed + destination.AverageSpeed) / 2;

        public IEnumerable<IAIPath> EndConnections => new[] { destination };

        public LaneType LaneType => destination.LaneType;

        public VehicleTypes VehicleTypes => destination.VehicleTypes;

        public float PathOffsetStart => 0.0f;

        public float PathOffsetEnd => 0.0f;

        public void Dispose()
        {
            source.DisconnectFrom(this);
        }
    }
}
