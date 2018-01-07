using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Trees;
using System;
using UnityEngine;

namespace Simulation.Traffic
{

    public class AISegment : Segment
    {
        private ISegmentAIPathsFactory aiPathFactory = SegmentAIPathsFactory.Default;
        private ISegmentPathFactory loftPathFactory = SegmentPathFactory.Default;
        private ILoftPath loftPath;
        private SegmentAIPath[] aiPaths;

        public AISegment(SegmentDescription description, RoadManager manager) : base(description, manager)
        {
        }

        public ILoftPath LoftPath => loftPath ?? (loftPath = createLoftPath());
        public SegmentAIPath[] AIPaths => aiPaths ?? (aiPaths = createAIPaths());

        private SegmentAIPath[] createAIPaths() => aiPathFactory.Create(this);

        private ILoftPath createLoftPath() => loftPathFactory.Create(this);

        private void InvalidateLoftPath() => loftPath = null;




        //public IRoadComponent<ILoftPath> LoftPath { get; }
        //public IRoadComponent<SegmentAIPath[]> AIPaths { get; }

        //public AISegment(SegmentDescription description, RoadManager manager, ISegmentAIPathsFactory aiFactory = null) : base(description, manager)
        //{
        //    LoftPath = new SegmentLoftPathComponent(this, SegmentPathFactory.Default);
        //    AIPaths = new SegmentAIPathComponent(this, aiFactory ?? SegmentAIPathsFactory.Default);
        //    Invalidated += LoftPath.Invalidate;
        //    LoftPath.Invalidated += AIPaths.Invalidate;
        //    AIPaths.Invalidated += InvalidateNodeAIPaths;
        //}

        //private void InvalidateNodeAIPaths()
        //{
        //    (Start.Node as AINode).InvalidateAIPaths(this);
        //}
    }

    public class Segment : IBoundsObject2D
    {
        public event Action Invalidated;
        public RoadManager Manager { get; internal set; }

        public SegmentDescription Description { get; }

        private SegmentNodeConnection start;
        private SegmentNodeConnection end;

        public SegmentNodeConnection Start
        {
            get => start;
            internal set => start = value;
        }
        public SegmentNodeConnection End
        {
            get => end;
            internal set => end = value;
        }
        public bool IsRemoved { get; internal set; }

        public Rect Bounds => getBounds();

        private Rect getBounds()
        {
            var a = Start.Node.Position;
            var b = End.Node.Position;

            var minX = Mathf.Min(a.x, b.x);
            var minY = Mathf.Min(a.y, b.y);

            var maxX = Mathf.Max(a.x, b.x);
            var maxY = Mathf.Max(a.y, b.y);

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        public Segment(SegmentDescription description, RoadManager manager)
        {
            this.Description = description;
            this.Manager = manager;
        }



        internal void NotifyOfMovement(SegmentNodeConnection segmentNodeConnection)
        {
            OnMoved();
            Manager.BoundsChanged(this);
            Invalidate();
        }

        protected virtual void OnMoved()
        {
        }

        //internal Segment(Node start, Node end, SegmentDescription description, RoadManager manager)
        //{
        //    this.Description = description;
        //    this.start = manager.Connect(this, start);
        //    this.end = manager.Connect(this, end);
        //    this.manager = manager;
        //}

        public void Invalidate()
        {
            Invalidated?.Invoke();
        }

        internal void NotifyOfRemoval()
        {
            OnRemoved();
        }


        internal void NotifyOfDisconnect(bool isStart)
        {
            if (isStart)
            {
                OnStartDisconnected();
                Start = null;
            }
            else
            {
                OnEndDisconnected();
                End = null;
            }

            Invalidate();
        }

        protected virtual void OnStartDisconnected()
        {
        }
        protected virtual void OnEndDisconnected()
        {
        }

        internal void NotifyOfCreation()
        {
            OnCreated();
        }

        protected virtual void OnCreated()
        {
        }
        protected virtual void OnRemoved()
        {
        }

    }
}