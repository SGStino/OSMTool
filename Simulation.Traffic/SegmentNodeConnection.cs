using System;
using UnityEngine;

namespace Simulation.Traffic
{
    public class SegmentNodeConnection
    {
        public SegmentNodeConnection Left
        {
            get { return left; }
            internal set { if (left != value) { left = value; OnLeftChanged(); } }
        }
        public SegmentNodeConnection Right
        {
            get { return right; }
            internal set { if (right != value) { right = value; OnRightChanged(); } }
        }

        protected virtual void OnRightChanged() { }
        protected virtual void OnLeftChanged() { }

        public RoadManager Manager { get; }

        public Node Node { get; }
        public Segment Segment { get; }

        public Vector3 Tangent
        {
            get { return tangent; }
            set
            {
                if (tangent != value)
                {
                    tangent = value;
                    if (isCreated)
                        OnTangentChanged();
                    Node.NotifyOfTangentChanged(this);
                }
            }
        }

        protected virtual void OnTangentChanged()
        {
        }

        private Vector3 tangent;
        private bool isCreated = false;
        private SegmentNodeConnection left;
        private SegmentNodeConnection right;

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
            left.Right = right;
            right.Left = left;
            OnDisconnected();
        }

        protected virtual void OnDisconnected()
        {
        }

        internal void NotifyOfCreation()
        {
            isCreated = true;
            OnCreated();
        }

        protected virtual void OnCreated()
        {

        }
    }
}