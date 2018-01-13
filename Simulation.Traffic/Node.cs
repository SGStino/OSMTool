using Simulation.Traffic.AI;
using Simulation.Traffic.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Utilities;

namespace Simulation.Traffic
{
    public static class NodeExtensions
    {
        public static void UpdateOffsets(this Node node)
        {
            int segmentCount = node.SegmentList.Count;

            if (segmentCount <= 0) return;
            if (segmentCount <= 1)
            {
                var radius = node.Segments.Max(l => l.Segment.GetWidth()) / 4;
                foreach (var seg in node.Segments)
                    seg.Offset = new Vector3(0, 0, radius);
            }
            else
            {
                var segments = node.Segments.OrderBy(t => Math.Atan2(t.Tangent.z, t.Tangent.x)).ToArray();
                float[] offsets = new float[segments.Length];

                for (int curr = 0; curr < segmentCount; curr++)
                {
                    var prev = (curr - 1);
                    if (prev < 0) prev += segmentCount;

                    if (getOffsets(node, segments[prev], segments[curr], out var offsetA, out var offsetB))
                    {
                        offsets[curr] = Mathf.Max(offsets[curr], offsetA);
                        offsets[prev] = Mathf.Max(offsets[prev], offsetB);
                    }
                }

                for (int i = 0; i < segmentCount; i++)
                {
                    var offset = offsets[i];
                    if (!float.IsNaN(offset) && !float.IsInfinity(offset))
                    {
                        segments[i].Offset = new Vector3(0, 0, offsets[i]);
                    }
                    else
                    {

                    }
                }
            }

        }

        private static bool getOffsets(Node node, SegmentNodeConnection con1, SegmentNodeConnection con2, out float offsetA, out float offsetB)
        {
            var wA = con1.Segment.GetWidth();
            var wB = con2.Segment.GetWidth();

            var o = node.Position.GetXZ();

            var dA = con1.Tangent.GetXZ();
            var dB = con2.Tangent.GetXZ();


            return VectorMath2D.IntersectsLineLine(o, dA, wA, o, dB, wB, out offsetA, out offsetB);

        }
    }

    public class AINode : Node
    {
        public AINode(Vector3 position, RoadManager manager, INodeAIPathsFactory aiFactory = null) : base(position, manager)
        {
        }

        public void InvalidateAIPaths(AISegment aISegment)
        {
        }

        public new IEnumerable<AISegmentNodeConnection> Segments => base.Segments.OfType<AISegmentNodeConnection>();

    }

    public class Node : IBoundsObject2D
    {
        internal IList<SegmentNodeConnection> SegmentList => segments;

        private List<SegmentNodeConnection> segments = new List<SegmentNodeConnection>();
        public RoadManager Manager { get; internal set; }

        private Vector3 position;

        public Node(Vector3 position, RoadManager manager)
        {
            this.Manager = manager;
            this.position = position;
        }


        public IEnumerable<SegmentNodeConnection> Segments => segments;

        public Rect Bounds
        {
            get
            {
                float radius = 1;

                Vector2 min, max;
                min.x = Position.x - radius;
                max.x = Position.x + radius;

                min.y = Position.y - radius;
                max.y = Position.y + radius;

                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        public Vector3 Position
        {
            get => position; set
            {
                position = value;
                NotifyOfMovement();
            }
        }

        internal void NotifyOfCreation()
        {
            OnCreated();
        }

        internal void NotifyOfMovement()
        {
            Invalidate();
            OnMoved();
            Manager.BoundsChanged(this);
            foreach (var con in segments)
                con.NotifyOfMovement();
        }

        public virtual void Invalidate()
        {
        }

        internal void NotifyOfOffsetChanged(SegmentNodeConnection segmentNodeConnection)
        {
            OnOffsetChanged(segmentNodeConnection);
        }

        protected virtual void OnOffsetChanged(SegmentNodeConnection segmentNodeConnection)
        {
        }

        protected virtual void OnMoved()
        {
        }


        protected virtual void OnCreated()
        {
        }

        internal void NotifyOfTangentChanged(SegmentNodeConnection segmentNodeConnection)
        {
            updateOrder();
            OnTangentChanged(segmentNodeConnection);
            Invalidate();
        }

        protected virtual void OnTangentChanged(SegmentNodeConnection segmentNodeConnection)
        {
        }

        private void updateOrder()
        {
            var segments = this.segments.OrderBy(getAngle).ToArray();
            for (int i = 0; i < segments.Length; i++)
            {
                var current = segments[i];
                var next = segments[(i + 1) % segments.Length];

                next.Left = current;
                current.Right = next;
            }
        }

        private float getAngle(SegmentNodeConnection arg)
        {
            return Mathf.Atan2(arg.Tangent.z, arg.Tangent.x);
        }

        /// <summary>
        /// Gets called by Manager
        /// </summary>
        internal void NotifyOfRemoval()
        {
            OnRemoved();
        }

        protected virtual void OnRemoved()
        {
        }

        internal void NotifiyOfDisconnect(SegmentNodeConnection connection)
        {
            updateOrder();
            OnDisconnected(connection);
            Invalidate();
        }

        protected virtual void OnDisconnected(SegmentNodeConnection connection)
        {
        }

        internal void NotifyOfConnection(SegmentNodeConnection connection)
        {
            updateOrder();
            OnConnected(connection);
            Invalidate();
        }

        protected virtual void OnConnected(SegmentNodeConnection connection)
        {
        }
    }
}
