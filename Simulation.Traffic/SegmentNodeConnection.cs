using System;
using UnityEngine;

namespace Simulation.Traffic
{
    public class SegmentNodeConnection
    {
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
                }
            }
        }

        protected virtual void OnTangentChanged()
        {
        }

        private Vector3 tangent;
        private bool isCreated = false;

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
            isCreated = true;
            OnCreated();
        }

        protected virtual void OnCreated()
        {

        }
    }
}