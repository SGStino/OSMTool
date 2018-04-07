using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using System.Threading;
using UnityEngine;
using System.Reactive.Linq;
using System;
using Simulation.Data;

namespace Simulation.Traffic.AI
{
    public class NodeAIPathsFactory : INodeAIPathsFactory
    {
        public static NodeAIPathsFactory Default { get; } = new NodeAIPathsFactory();

        //public NodeAIPath[] Create(Node node)
        //{
        //    var nodeRadius = 10;

        //    var segments = node.Segments.Select(t => t.Segment).OfType<Segment>().Where(t => t.Start != null && t.End != null).ToArray();


        //    var nodePaths = new List<NodeAIPath>();
        //    for (int iA = 1; iA < segments.Length; iA++)
        //    {
        //        for (int iB = 0; iB < iA; iB++)
        //        {
        //            var segA = segments[iA];
        //            var loftA = segA.LoftPath.Value;
        //            var segB = segments[iB];
        //            var loftB = segB.LoftPath.Value;


        //            var isAStart = segA.End?.Node == node;
        //            var isBStart = segB.Start?.Node == node;

        //            var offsetA = isAStart ? nodeRadius : loftA.Length - nodeRadius;
        //            var offsetB = isBStart ? nodeRadius : loftB.Length - nodeRadius;

        //            var headingA = isAStart ? segA.Start.GetHeading() : segB.End.GetHeading();
        //            var headingB = isBStart ? segB.Start.GetHeading() : segB.End.GetHeading();

        //            var pointA = loftA.GetTransformedPoint(offsetA, Vector3.zero);
        //            var pointB = loftB.GetTransformedPoint(offsetB, Vector3.zero);

        //            var loft = new BiArcLoftPath(pointA, headingA, pointB, headingB);

        //            //nodePaths.Add()
        //        }
        //    }

        //    return nodePaths.ToArray();
        //}

        public NodeAIRoute[] CreateRoutes(ISegmentNodeConnection from, ISegmentNodeConnection to)
        {


            var incomingRoutes = from.Segment.AIRoutes.Where(r => r.GetEnd() == from).ToArray();
            var outgoingRoutes = to.Segment.AIRoutes.Where(r => r.GetStart() == to).ToArray();

            var routes = new List<NodeAIRoute>();

            foreach (var incoming in incomingRoutes)
            {
                foreach (var outgoing in outgoingRoutes)
                {
                    var paths = create(incoming, outgoing);
                    if (paths.Any())
                    {
                        var node = new NodeAIRoute(from, outgoing, paths.ToArray());

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

            var inPaths = incoming.Paths.OrderBy(t => Mathf.Abs(t.Offsets.Value.SideOffsetStart)).ToArray();
            var outPaths = outgoing.Paths.OrderBy(t => Mathf.Abs(t.Offsets.Value.SideOffsetStart)).ToArray();


            if (inPaths.Length == outPaths.Length)
            {
                for (int i = 0; i < inPaths.Length; i++)
                {
                    var iPath = inPaths[i];
                    var oPath = outPaths[i];
                     

                    var loftPath = createLineLoft(iPath, from, oPath, to).ToObservableValue();
                    yield return new NodeAIPath(iPath, oPath, loftPath); 
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
                                var loftPath = createLineLoft(iPath, from, oPath, to).ToObservableValue();
                                yield return new NodeAIPath(iPath, oPath, loftPath);
                            }
                        }
                }
            }
        }

        private float getAngle(ISegmentNodeConnection from, ISegmentNodeConnection to)
        {
            var fromTangent = from.Offset.Value.Tangent;
            var toTangent = to.Offset.Value.Tangent;

            var a2d = new Vector2(fromTangent.x, fromTangent.z);
            var b2d = -new Vector2(toTangent.x, toTangent.z);

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

        private IObservable<ILoftPath> createLineLoft(SegmentAIPath iPath, ISegmentNodeConnection from, SegmentAIPath oPath, ISegmentNodeConnection to)
        {

            var end = iPath.ObserveTransform(1, absolute: false);
            var start = iPath.ObserveTransform(0, absolute: false);


            var loft = end.CombineLatest<Matrix4x4, Matrix4x4, ILoftPath>(start, (t1, t2) =>
            {
                var point1 = t1.MultiplyPoint3x4(Vector3.zero);
                var point2 = t2.MultiplyPoint3x4(Vector3.zero);

                var f1 = t1.MultiplyVector(Vector3.forward).normalized;
                var f2 = t2.MultiplyVector(Vector3.forward).normalized;
                return new BiArcLoftPath(point1, -f1, point2, -f2);
            });

            return loft;
        }

        //private static Vector3 GetPoint(SegmentAIPath iPath, bool end)
        //{
        //    var point = new Vector3(iPath.Reverse ^ end ? iPath.PathOffsetStart : iPath.PathOffsetEnd, 0, 0);
        //    var p = iPath.Reverse ^ end ? iPath.LoftPath.Value.Length : 0;
        //    point = iPath.LoftPath.Value.GetTransformedPoint(p, point);
        //    return point;
        //}

        //private IEnumerable<SegmentAIRoute> GetRoutes(ISegmentNodeConnection con, bool incoming)
        //{
        //    var isEnd = con.Segment.End == con;



        //    return GetRoutes(isEnd ^ incoming, con.Segment);
        //}

        //private static IEnumerable<SegmentAIRoute> GetRoutes(bool isEnd, ISegment segment)
        //{
        //    return segment.AIRoutes.Where(t => t.Reverse != isEnd);
        //}
    }
}
