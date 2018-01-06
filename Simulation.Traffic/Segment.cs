using Simulation.Traffic.Lofts;
using Simulation.Traffic.Trees;
using System;
using UnityEngine;

namespace Simulation.Traffic
{
    public class Segment : IBoundsObject2D
    {
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

        public IRoadComponent<ILoftPath> Path { get; }

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

        public Segment(SegmentDescription description, RoadManager manager, IComponentValueFactory<ILoftPath, Segment> pathFactory = null)
        {
            this.Description = description;
            this.Manager = manager;

            Path = new FactoryComponent<ILoftPath, Segment>(this, pathFactory ?? SegmentPathFactory.Default);
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
            Path.Invalidate();
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