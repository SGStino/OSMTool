using Simulation.Traffic.AI;
using System;
using System.Linq;
using UnityEngine;

namespace Simulation.Traffic
{
    public class AISegmentNodeConnection : SegmentNodeConnection
    {
        private INodeAIPathsFactory aiPathFactory = NodeAIPathsFactory.Default;
        private NodeAIRoute[] aiRoutes;

        public AISegmentNodeConnection(AISegment segment, AINode node, AIRoadManager manager) : base(segment, node, manager)
        {
        }

        public NodeAIRoute[] AIRoutes => aiRoutes ?? (aiRoutes = createAIRoutes());

        private NodeAIRoute[] createAIRoutes() => aiPathFactory.CreateRoutes(this);

        public new AISegmentNodeConnection Left { get => base.Left as AISegmentNodeConnection; internal set => base.Left = value; }
        public new AISegmentNodeConnection Right { get => base.Right as AISegmentNodeConnection; internal set => base.Right = value; }

        public new AINode Node => base.Node as AINode;
        public new AISegment Segment => base.Segment as AISegment;


        internal void InvalidateRoutes()
        {
            var oldRoutes = aiRoutes;
            aiRoutes = null;
            if (oldRoutes != null)
                foreach (var route in oldRoutes)
                    route.Dispose();
        }

        protected override void OnTangentChanged()
        {
            InvalidateRoutes();
        }

        public override void Invalidate()
        {
            base.Invalidate();
            InvalidateRoutes();
        }
    }

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

        public Vector3 Offset
        {
            get { return offset; }
            set
            {
                if (offset != value)
                {
                    offset = value;
                    if (isCreated)
                        OnOffsethanged();
                    Node.NotifyOfOffsetChanged(this);
                    Segment.NotifyOfOffsetChanged(this);
                }
            }
        }

        protected virtual void OnOffsethanged()
        {
        }

        public Vector3 Tangent
        {
            get { return tangent; }
            set
            {
                if (value.magnitude == 0) throw new InvalidOperationException();
                if (tangent != value)
                {
                    tangent = value;
                    if (isCreated)
                        OnTangentChanged();
                    Node.NotifyOfTangentChanged(this);
                    Segment.NotifyOfTangentChanged(this);
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
        private Vector3 offset;



        public SegmentNodeConnection(Segment segment, Node node, RoadManager manager)
        {
            this.Segment = segment;
            this.Node = node;
            this.Manager = manager;
            
            offset = new Vector3(0, 0, 5);
        }

        internal void NotifyOfMovement()
        {
            OnMoved();
            Segment.NotifyOfMovement(this);
        }

        protected virtual void OnMoved()
        {
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

        public virtual void Invalidate()
        {

        }
    }
}