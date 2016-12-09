using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic
{
    public class RoadManager
    {
        private List<Node> nodes = new List<Node>();
        private List<Segment> segments = new List<Segment>();

        public IEnumerable<Node> Nodes => nodes;
        public IEnumerable<Segment> Segments => segments;

        public Node CreateNodeAt(float x, float y)
        {
            return CreateNode(new Vector3(x, y, 0));
        }

        public Node CreateNode(Vector3 position)
        {
            var node = createNode(position);
            nodes.Add(node);
            node.NotifyOfCreation();
            return node;
        }

        public Segment CreateSegment(Node start, Node end, SegmentDescription description)
        {
            var segment = createSegment(start, end, description);

            segment.Start = Connect(segment, start);
            segment.End = Connect(segment, end);

            segments.Add(segment);
            segment.NotifyOfCreation();
            return segment;
        }


        internal SegmentNodeConnection Connect(Segment segment, Node start)
        {
            var connection = createConnection(segment, start);
            lock (start.SegmentList)
            {
                start.SegmentList.Add(connection);
            }
            start.NotifyOfConnection(connection);
            connection.NotifyOfCreation();
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
            if (segment.Manager != this)
                throw new InvalidOperationException("Remove failed: This node is not owned by this manager");
            if (segments.Remove(segment))
            {
                segment.NotifyOfRemoval();
                segment.Manager = null;
                disconnect(segment.Start);
                segment.Start = null;
                disconnect(segment.End);
                segment.End = null;
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
