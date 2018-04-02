using Simulation.Traffic.AI;
using System;
using System.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Simulation.Traffic
{
    //public class AISegmentNodeConnection : SegmentNodeConnection
    //{
    //    private INodeAIPathsFactory aiPathFactory = NodeAIPathsFactory.Default;
    //    private NodeAIRoute[] aiRoutes;

    //    public AISegmentNodeConnection(AISegment segment, AINode node, AIRoadManager manager) : base(segment, node, manager)
    //    {
    //    }

    //    public NodeAIRoute[] AIRoutes => aiRoutes ?? (aiRoutes = createAIRoutes());

    //    private NodeAIRoute[] createAIRoutes() => aiPathFactory.CreateRoutes(this);

    //    public new AISegmentNodeConnection Left { get => base.Left as AISegmentNodeConnection; internal set => base.Left = value; }
    //    public new AISegmentNodeConnection Right { get => base.Right as AISegmentNodeConnection; internal set => base.Right = value; }

    //    public new AINode Node => base.Node as AINode;
    //    public new AISegment Segment => base.Segment as AISegment;


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
        INode Node { get; }
        Vector3 Offset { get; }
        Vector3 Tangent { get; }
    }
    public class SegmentNodeConnection : ISegmentNodeConnection
    {
        private readonly Subject<SegmentNodeConnectionEvent> localEvents = new Subject<SegmentNodeConnectionEvent>();


        private Vector3 tangent;
        private Vector3 offset;


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

         

        public SegmentNodeConnection(Node node, Segment segment)
        {
            Node = node;
            Segment = segment;
            offset = new Vector3(0, 0, 5);
        }
    }
}