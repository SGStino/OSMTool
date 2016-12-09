using System;
using UnityEngine;

namespace Simulation.Traffic
{
    public class SegmentNodeConnection
    {
        public RoadManager Manager { get; }

        public Node Node { get; }
        public Segment Segment { get; }

        public Vector2 Tangent
        {
            get { return tangent; }
            set { tangent = value; OnTangentChanged(); }
        }

        protected virtual void OnTangentChanged()
        {
        }

        private Vector2 tangent;

        public SegmentNodeConnection(Segment segment, Node node, RoadManager manager)
        {
            this.Segment = segment;
            this.Node = node;
            this.Manager = manager;
        }
        /// <summary>
        /// gets called from segment
        /// </summary>
        internal void NotifyOfDisconnect()
        {
            OnDisconnected();
        }

        protected virtual void OnDisconnected()
        {
        }

        internal void NotifyOfCreation()
        {
            OnCreated();
        }

        protected virtual void OnCreated()
        {
 
        }
    }
}