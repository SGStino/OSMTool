using Simulation.Data;
using Simulation.Traffic.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Simulation.Traffic
{
    //public class SegmentNodeConnection : SegmentNodeConnection
    //{
    //    private INodeAIPathsFactory aiPathFactory = NodeAIPathsFactory.Default;
    //    private NodeAIRoute[] aiRoutes;

    //    public SegmentNodeConnection(Segment segment, Node node, AIRoadManager manager) : base(segment, node, manager)
    //    {
    //    }

    //    public NodeAIRoute[] AIRoutes => aiRoutes ?? (aiRoutes = createAIRoutes());

    //    private NodeAIRoute[] createAIRoutes() => aiPathFactory.CreateRoutes(this);

    //    public new SegmentNodeConnection Left { get => base.Left as SegmentNodeConnection; internal set => base.Left = value; }
    //    public new SegmentNodeConnection Right { get => base.Right as SegmentNodeConnection; internal set => base.Right = value; }

    //    public new Node Node => base.Node as Node;
    //    public new Segment Segment => base.Segment as Segment;


    //    internal void InvalidateRoutes()
    //    {
    //        var oldRoutes = aiRoutes;
    //        aiRoutes = null;
    //        if (oldRoutes != null)
    //            foreach (var route in oldRoutes)
    //                route.Dispose();
    //    }

    //    protected override void OnTangentChanged()
    //    {
    //        base.OnTangentChanged();
    //        InvalidateRoutes();
    //    }

    //    protected override void OnMoved()
    //    {
    //        base.OnMoved();
    //        InvalidateRoutes();
    //    }

    //    public override void Invalidate()
    //    {
    //        base.Invalidate();
    //        InvalidateRoutes();
    //    }
    //}

    public struct SegmentNodeConnectionEvent
    {
        public SegmentNodeConnectionEvent(Vector3 offset, Vector3 tangent, SegmentNodeConnectionEventType type) : this()
        {
            Offset = offset;
            Tangent = tangent;
            Type = type;
        }

        public Vector3 Offset { get; }
        public Vector3 Tangent { get; }
        public SegmentNodeConnectionEventType Type { get; }

        public static SegmentNodeConnectionEvent TangentChanged(Vector3 offset, Vector3 tangent) => new SegmentNodeConnectionEvent(offset, tangent, SegmentNodeConnectionEventType.TangentChanged);
        public static SegmentNodeConnectionEvent OffsetChanged(Vector3 offset, Vector3 tangent) => new SegmentNodeConnectionEvent(offset, tangent, SegmentNodeConnectionEventType.OffsetChanged);
        public static SegmentNodeConnectionEvent Disconnected(Vector3 offset, Vector3 tangent) => new SegmentNodeConnectionEvent(offset, tangent, SegmentNodeConnectionEventType.Disconnected);
    }

    public enum SegmentNodeConnectionEventType : byte
    {
        TangentChanged,
        OffsetChanged,
        Disconnected
    }

    public interface ISegmentNodeConnection : IObservable<SegmentNodeConnectionEvent>, IDisposable
    {
        ISegment Segment { get; }
        INode Node { get; }
        Vector3 Offset { get; set; }
        Vector3 Tangent { get; set; }
        IObservableValue<IReadOnlyList<IAIRoute>> OutgoingAIRoutes { get; }
        IObservableValue<IReadOnlyList<IAIRoute>> IncomingAIRoutes { get; }
    }
    public class SegmentNodeConnection : ISegmentNodeConnection
    {
        private readonly Subject<SegmentNodeConnectionEvent> localEvents = new Subject<SegmentNodeConnectionEvent>();

        private Vector3 tangent;
        private Vector3 offset;


        public INode Node { get; }
        public ISegment Segment { get; private set; }

        public Vector3 Offset
        {
            get { return offset; }
            set
            {
                if (offset != value)
                {
                    offset = value;
                    RaiseEvent(SegmentNodeConnectionEvent.OffsetChanged(value, tangent));
                }
            }
        }

        private void RaiseEvent(SegmentNodeConnectionEvent segmentNodeConnectionEvent) => localEvents.OnNext(segmentNodeConnectionEvent);

        public void Dispose()
        {
            RaiseEvent(SegmentNodeConnectionEvent.Disconnected(offset, tangent));
            localEvents.OnCompleted();
            localEvents.Dispose();
        }

        public IDisposable Subscribe(IObserver<SegmentNodeConnectionEvent> observer) => localEvents.Subscribe(observer);

        public Vector3 Tangent
        {
            get { return tangent; }
            set
            {
                if (value.magnitude == 0) throw new InvalidOperationException();
                if (tangent != value)
                {
                    tangent = value;
                    RaiseEvent(SegmentNodeConnectionEvent.TangentChanged(offset, tangent));
                }
            }
        }

        // TODO: update based on connections in node
        public IObservableValue<IReadOnlyList<IAIRoute>> OutgoingAIRoutes => throw new NotImplementedException();

        public IObservableValue<IReadOnlyList<IAIRoute>> IncomingAIRoutes => throw new NotImplementedException();

        public SegmentNodeConnection(INode node)
        {
            Node = node;
            offset = new Vector3(0, 0, 5);
        }

        internal void SetSegment(ISegment segment)
        {
            if (Segment == null)
                this.Segment = segment;
            else
                throw new InvalidOperationException("Segment already set");
        }
    }
}