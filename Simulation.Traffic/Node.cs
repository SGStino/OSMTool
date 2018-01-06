using Simulation.Traffic.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic
{
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
            OnMoved();
            Manager.BoundsChanged(this);
            foreach (var con in segments)
                con.NotifyOfMovement();
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
        }

        protected virtual void OnDisconnected(SegmentNodeConnection connection)
        {
        }

        internal void NotifyOfConnection(SegmentNodeConnection connection)
        {
            updateOrder();
            OnConnected(connection);
        }

        protected virtual void OnConnected(SegmentNodeConnection connection)
        {
        }
    }
}
