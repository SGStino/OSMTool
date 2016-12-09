using System;

namespace Simulation.Traffic
{
    public class Segment
    {
        public RoadManager Manager { get; internal set; }

        public SegmentDescription Description { get; }
        public SegmentNodeConnection Start { get; internal set; }
        public SegmentNodeConnection End { get; internal set; }



        public Segment(SegmentDescription description, RoadManager manager)
        {
            this.Description = description;
            this.Manager = manager;
        }

        //internal Segment(Node start, Node end, SegmentDescription description, RoadManager manager)
        //{
        //    this.Description = description;
        //    this.start = manager.Connect(this, start);
        //    this.end = manager.Connect(this, end);
        //    this.manager = manager;
        //}


        internal void NotifyOfRemoval()
        {
            OnRemoved();
        }

        protected virtual void OnRemoved()
        {

        }

        internal void NotifyOfDisconnect(bool isStart)
        {
            if (isStart)
                OnStartDisconnected();
            else
                OnEndDisconnected();
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
    }
}