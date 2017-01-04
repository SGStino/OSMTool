using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic
{
    public class Node
    {
        internal IList<SegmentNodeConnection> SegmentList => segments;

        private List<SegmentNodeConnection> segments = new List<SegmentNodeConnection>();
        public RoadManager Manager { get; internal set; }

        public Vector3 Position { get; }

        public Node(Vector3 position, RoadManager manager)
        {
            this.Manager = manager;
            this.Position = position;
        }


        public IEnumerable<SegmentNodeConnection> Segments => segments;

        internal void NotifyOfCreation()
        {
            OnCreated();
        }

        protected virtual void OnCreated()
        {
        }

        internal void OnTangentChanged(SegmentNodeConnection segmentNodeConnection)
        {
            var segments = this.segments.OrderBy(getAngle).ToArray();
            for(int i = 0; i < segments.Length; i++)
            {
                var current = segments[i];
                var next = segments[(i + 1) % segments.Length];

                next.Left = current;
                current.Right = next;
            }
        }

        private float getAngle(SegmentNodeConnection arg)
        {
            return Mathf.Atan2(arg.Node.Position.z, arg.Node.Position.x);
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
            OnDisconnected(connection);
        }

        protected virtual void OnDisconnected(SegmentNodeConnection connection)
        {
        }

        internal void NotifyOfConnection(SegmentNodeConnection connection)
        {
            OnConnected(connection);
        }

        protected virtual void OnConnected(SegmentNodeConnection connection)
        { 
        }
    }
}
