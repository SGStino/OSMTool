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
            var incomingRoutes = con.Segment.AIRoutes.Where(r => r.GetEnd()?.Node == con.Node).ToArray();

            var allRoutes = con.Node.Segments.SelectMany(t => t.Segment.AIRoutes);
            var outgoingRoutes = allRoutes.Where(r => r.GetStart()?.Node == con.Node).ToArray();

            var routes = new List<NodeAIRoute>();

            foreach (var incoming in incomingRoutes)
            {
                foreach (var outgoing in outgoingRoutes)
                {
                    var paths = create(incoming, outgoing);
                    if (paths.Any())
                    {
                        var node = new NodeAIRoute(con, outgoing, paths.ToArray());

                        routes.Add(node);
                    }
                }
            }
            return routes.ToArray();
        }

        private IEnumerable<NodeAIPath> create(SegmentAIRoute incoming, SegmentAIRoute outgoing)
        {
            var from = incoming.GetEnd();
            var to = outgoing.GetStart();


            if (incoming.Paths.Length == outgoing.Paths.Length)
            {
                for (int i = 0; i < incoming.Paths.Length; i++)
                {
                    var iPath = incoming.Paths[i];
                    var oPath = outgoing.Paths[i];
                    yield return new NodeAIPath(iPath, oPath, createLineLoft(iPath, from, oPath, to));
                }
            }
            else
            {
                float angle = getAngle(from, to);

                int index = Mathf.RoundToInt(angle * 16 / (2 * Mathf.PI));

                if (index < 0) index += 16;
                if (index >= 16) index -= 16;

                foreach (var iPath in incoming.Paths)
                {
                    var turn = iPath.Lane.Turn;

                    var angleMap = getAngleMap(turn);

                    bool accept = angleMap == null || angleMap[index];
                    if (accept)
                        foreach (var oPath in outgoing.Paths)
                        {

                            if (iPath.LoftPath != null && oPath.LoftPath != null)
                            {

                                yield return new NodeAIPath(iPath, oPath, createLineLoft(iPath, from, oPath, to));
                            }
                        }
                }
            }
        }

        private float getAngle(AISegmentNodeConnection from, AISegmentNodeConnection to)
        {
            var a2d = new Vector2(from.Tangent.x, from.Tangent.z);
            var b2d = -new Vector2(to.Tangent.x, to.Tangent.z);

            return Mathf.Atan2(a2d.y, a2d.x) - Mathf.Atan2(b2d.y, b2d.x);
        }

        private static bool[] getAngleMap(Turn turn)
        {
            bool[] angleMap = new bool[16];

            if (turn.HasFlag(Turn.Through))
                angleMap[0] = angleMap[1] = angleMap[14] = angleMap[15] = true;
            if (turn.HasFlag(Turn.MergeLeft))
                angleMap[14] = angleMap[15] = true;
            if (turn.HasFlag(Turn.MergeRight))
                angleMap[0] = angleMap[1] = true;
            if (turn.HasFlag(Turn.SlightLeft))
                angleMap[13] = angleMap[14] = true;
            if (turn.HasFlag(Turn.SlightRight))
                angleMap[1] = angleMap[2] = true;
            if (turn.HasFlag(Turn.Left))
                angleMap[10] = angleMap[11] = angleMap[12] = angleMap[13] = true;
            if (turn.HasFlag(Turn.Right))
                angleMap[2] = angleMap[3] = angleMap[4] = angleMap[5] = true;
            if (turn.HasFlag(Turn.SharpLeft))
                angleMap[9] = angleMap[10] = true;
            if (turn.HasFlag(Turn.SharpRight))
                angleMap[5] = angleMap[6] = true;
            if (turn.HasFlag(Turn.Reverse))
                angleMap[7] = angleMap[8] = true;

            if (turn == Turn.None)
                return null;
            return angleMap;
        }

        private ILoftPath createLineLoft(SegmentAIPath iPath, AISegmentNodeConnection from, SegmentAIPath oPath, AISegmentNodeConnection to)
        {
            var point1 = from.GetPosition();
            var point2 = to.GetPosition();
            return new BiArcLoftPath(point1, from.Tangent, point2, -to.Tangent);
        }

        private static Vector3 GetPoint(SegmentAIPath iPath, bool end)
        {
            var point = new Vector3(iPath.Reverse ^ end ? iPath.PathOffsetStart : iPath.PathOffsetEnd, 0, 0);
            var p = iPath.Reverse ^ end ? iPath.LoftPath.Length : 0;
            point = iPath.LoftPath.GetTransformedPoint(p, point);
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
}
