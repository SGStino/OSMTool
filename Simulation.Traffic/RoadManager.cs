using Simulation.Traffic;
using Simulation.Traffic.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Simulation.Traffic
{
    public class RoadManager
    {



        private QuadTree<Node> nodes = new QuadTree<Node>(new Rect(0, 0, 1000, 1000));
        private QuadTree<Segment> segments = new QuadTree<Segment>(new Rect(0, 0, 1000, 1000));

        public IEnumerable<Node> Nodes => nodes.Items;
        public IEnumerable<Segment> Segments => segments.Items;

        public IEnumerable<Segment> QuerySegments(Rect bounds) => segments.Query(bounds);
        public IEnumerable<Segment> QuerySegments(Vector2 point, float radius) => segments.Query(point, radius);


        public IEnumerable<Node> QueryNodes(Rect bounds) => nodes.Query(bounds);
        public IEnumerable<Node> QueryNodes(Vector2 point, float radius) => nodes.Query(point, radius);

        public Node CreateNodeAt(float x, float y)
        {
            return CreateNode(new Vector3(x, 0, y));
        }

        public Node CreateNode(Vector3 position)
        {
            var node = createNode(position);
            nodes.Add(node);
            node.NotifyOfCreation();
            return node;
        }

        internal void BoundsChanged(Node node)
        {
            nodes.Update(node);
        }

        public Segment CreateSegment(Node start, Node end, SegmentDescription description)
        {
            var segment = createSegment(start, end, description);

            var dir = start.Position - end.Position;
            dir = dir.normalized;

            segment.Start = connect(segment, start, -dir);
            segment.End = connect(segment, end, dir);

            if (segment.Start == null || segment.End == null) throw new InvalidOperationException("Segments need starts and ends!");

            segments.Add(segment);
            segment.NotifyOfCreation();

            notifyConnect(start, segment.Start);
            notifyConnect(end, segment.End);

            return segment;
        }

        internal void BoundsChanged(Segment segment)
        {
            segments.Update(segment);
        }


        private void notifyConnect(Node node, SegmentNodeConnection connection)
        {
            node.NotifyOfConnection(connection);
            connection.NotifyOfCreation();
        }

        public Segment MergeSegments(Node node)
        {
            if (node.SegmentList.Count != 2) throw new InvalidOperationException("can only merge 2 segements");

            var a = node.SegmentList[0];
            var b = node.SegmentList[1];

            var segA = a.Segment;
            var segB = b.Segment;

            Segment segment;


            bool isAStart = segA.Start == a;
            bool isBStart = segB.Start == b;

            if (isAStart == isBStart) throw new InvalidOperationException("can only merge nodes if the roads follow the same direction");


            SegmentNodeConnection otherA = isAStart ? segA.End : segA.Start;
            SegmentNodeConnection otherB = isBStart ? segB.End : segB.Start;

            if (!isAStart)
            {
                segment = CreateSegment(otherA.Node, otherB.Node, segA.Description);
                segment.Start.Tangent = otherA.Tangent;
                segment.End.Tangent = otherB.Tangent;
            }
            else
            {
                segment = CreateSegment(otherB.Node, otherA.Node, segA.Description);
                segment.Start.Tangent = otherB.Tangent;
                segment.End.Tangent = otherA.Tangent;
            }

            Remove(node);
            return segment;
        }

        public Node MergeNodes(Node a, Node b)
        {
            var aSegments = a.Segments;
            var bSegments = b.Segments;

            var newPosition = (a.Position + b.Position) / 2;
            var newNode = CreateNode(newPosition);

            foreach (var segment in aSegments.Concat(bSegments))
            {
                var start = segment.Segment.Start;
                var end = segment.Segment.End;
                if (start != null && end != null)
                {
                    if (start.Node == a && end.Node != b && end.Node != a)
                        CreateSegment(newNode, end.Node, segment.Segment.Description);

                    if (start.Node == b && end.Node != a && end.Node != b)
                        CreateSegment(newNode, end.Node, segment.Segment.Description);

                    if (end.Node == a && start.Node != b && start.Node != a)
                        CreateSegment(start.Node, newNode, segment.Segment.Description);

                    if (end.Node == b && start.Node != a && start.Node != b)
                        CreateSegment(start.Node, newNode, segment.Segment.Description);
                }
            }
            Remove(a);
            Remove(b);
            return newNode;
        }



        private SegmentNodeConnection connect(Segment segment, Node start, Vector3 tangent)
        {
            var connection = createConnection(segment, start);
            connection.Tangent = tangent;
            lock (start.SegmentList)
            {
                start.SegmentList.Add(connection);
            }
            return connection;
        }

        protected virtual Node createNode(Vector3 position)
        {
            return new Node(position, this);
        }

        protected virtual Segment createSegment(Node start, Node end, SegmentDescription description)
        {
            return new Segment(description, this);
        }

        protected virtual SegmentNodeConnection createConnection(Segment segment, Node start)
        {
            return new SegmentNodeConnection(segment, start, this);
        }

        public bool Remove(Segment segment)
        {
            if (segment.IsRemoved) return false;
            if (segment.Manager != this)
                throw new InvalidOperationException("Remove failed: This node is not owned by this manager");

            segment.IsRemoved = true;
            if (segments.Remove(segment))
            {
                segment.NotifyOfRemoval();
                disconnect(segment.Start);
                segment.Start = null;
                disconnect(segment.End);
                segment.End = null;
                segment.Manager = null;
                return true;
            }
            return false;
        }

        public bool Remove(Node node)
        {
            if (nodes.Remove(node))
            {
                for (int i = node.SegmentList.Count - 1; i >= 0; --i)
                    disconnect(node.SegmentList[i]);
                node.Manager = null;
                node.NotifyOfRemoval();
                return true;
            }
            return false;
        }

        private void disconnect(SegmentNodeConnection connection)
        {
            if (connection == null) return; //already disconnected

            var segments = connection.Node.SegmentList;
            lock (segments)
            {
                bool isStart = connection.Segment.Start == connection;
                connection.Node.NotifiyOfDisconnect(connection);
                segments.Remove(connection);

                connection.Segment.NotifyOfDisconnect(isStart);
                if (isStart)
                    connection.Segment.Start = null;
                else
                    connection.Segment.End = null;
            }
            Remove(connection.Segment);
        }
    }
}
