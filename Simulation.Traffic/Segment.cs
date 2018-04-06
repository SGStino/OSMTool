using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using Simulation.Data.Trees;
using System;
using UnityEngine;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive;
using System.Linq;
using Simulation.Data;
using System.Collections.Generic;

namespace Simulation.Traffic
{

    //public class Segment : Segment
    //{
    //    private ISegmentAIPathsFactory aiPathFactory = SegmentAIPathsFactory.Default;
    //    private ISegmentPathFactory loftPathFactory = SegmentPathFactory.Default;
    //    private ILoftPath loftPath;
    //    private SegmentAIRoute[] aiRoutes;

    //    public Segment(SegmentDescription description, AIRoadManager manager) : base(description, manager)
    //    {
    //    }

    //    public ILoftPath LoftPath => loftPath ?? (loftPath = createLoftPath());
    //    public SegmentAIRoute[] AIRoutes => aiRoutes ?? (aiRoutes = createAIRoutes());

    //    private SegmentAIRoute[] createAIRoutes() => aiPathFactory.CreateRoutes(this);

    //    private ILoftPath createLoftPath() => loftPathFactory.Create(this);

    //    private void InvalidateLoftPath() => loftPath = null;



    //    public new SegmentNodeConnection Start
    //    {
    //        get => base.Start as SegmentNodeConnection;
    //        internal set => base.Start = value;
    //    }
    //    public new SegmentNodeConnection End
    //    {
    //        get => base.End as SegmentNodeConnection;
    //        internal set => base.End = value;
    //    }

    //    protected override void OnOffsetChanged(SegmentNodeConnection segmentNodeConnection)
    //    {
    //        base.OnOffsetChanged(segmentNodeConnection);
    //        InvalidateLoftPath();
    //    }

    //    protected override void OnMoved()
    //    {
    //        base.OnMoved();
    //        InvalidateLoftPath();
    //    }


    //    //public IRoadComponent<ILoftPath> LoftPath { get; }
    //    //public IRoadComponent<SegmentAIPath[]> AIPaths { get; }

    //    //public Segment(SegmentDescription description, RoadManager manager, ISegmentAIPathsFactory aiFactory = null) : base(description, manager)
    //    //{
    //    //    LoftPath = new SegmentLoftPathComponent(this, SegmentPathFactory.Default);
    //    //    AIPaths = new SegmentAIPathComponent(this, aiFactory ?? SegmentAIPathsFactory.Default);
    //    //    Invalidated += LoftPath.Invalidate;
    //    //    LoftPath.Invalidated += AIPaths.Invalidate;
    //    //    AIPaths.Invalidated += InvalidateNodeAIPaths;
    //    //}

    //    //private void InvalidateNodeAIPaths()
    //    //{
    //    //    (Start.Node as Node).InvalidateAIPaths(this);
    //    //}
    //}

    public interface ISegment : IObservable<SegmentEvent>, IDisposable, IBoundsObject2D
    {
        SegmentDescription Description { get; }
        //IObservable<ILoftPath> LoftPath { get; } 
        IObservableValue<ILoftPath> LoftPath { get; }

        IReadOnlyList<IAIRoute> AIRoutes { get; }

        ISegmentNodeConnection Start { get; }
        ISegmentNodeConnection End { get; }
    }

    public struct SegmentEvent
    {
        public SegmentEvent(SegmentEventType type) : this()
        {
            Type = type;
        }

        public SegmentEventType Type { get; }

        public static SegmentEvent Disconnect() => new SegmentEvent(SegmentEventType.Disconnected);
    }

    public enum SegmentEventType
    {
        Disconnected
    }

    public class Segment : ISegment, IObservable<SegmentEvent>, IDisposable
    {
        private readonly Subject<SegmentEvent> localEvents = new Subject<SegmentEvent>();
        private readonly IObservable<((Vector3 position, Vector3 offset, Vector3 tangent) start, (Vector3 position, Vector3 offset, Vector3 tangent) end)> _shapeChange;
        private readonly BehaviorSubjectValue<ILoftPath> _loftPath;

        public IObservableValue<ILoftPath> LoftPath => _loftPath;

        private readonly CompositeDisposable dispose = new CompositeDisposable();
        private readonly IObservableValue<Rect> _bounds;


        public SegmentDescription Description { get; }


        public ISegmentNodeConnection Start { get; }
        public ISegmentNodeConnection End { get; }


        public IObservableValue<Rect> Bounds => _bounds;

        public IReadOnlyList<IAIRoute> AIRoutes { get; }

        //private Rect getBounds()
        //{
        //    var a = Start.Node.Position;
        //    var b = End.Node.Position;

        //    var minX = Mathf.Min(a.x, b.x);
        //    var minY = Mathf.Min(a.y, b.y);

        //    var maxX = Mathf.Max(a.x, b.x);
        //    var maxY = Mathf.Max(a.y, b.y);

        //    return Rect.MinMaxRect(minX, minY, maxX, maxY);
        //}

        public Segment(SegmentDescription description, ISegmentNodeConnection start, ISegmentNodeConnection end, IObservable<Unit> sampler = null)
        {
            this.Description = description;
            Start = start;
            End = end;


            var startPosition = start
                .Node
                .Where(t => t.Type == NodeChangeEventType.Moved)
                .Select(t => t.Position)
                .StartWith(end.Node.Position);

            var endPosition = end
                .Node
                .Where(t => t.Type == NodeChangeEventType.Moved)
                .Select(t => t.Position)
                .StartWith(end.Node.Position);

            var startOffsetTangent = start
                .Where(t => t.Type == SegmentNodeConnectionEventType.OffsetChanged | t.Type == SegmentNodeConnectionEventType.TangentChanged)
                .Select(t => (offset: t.Offset, tangent: t.Tangent))
                .StartWith((offset: start.Offset, tangent: start.Tangent));

            var endOffsetTangent = end
                .Where(t => t.Type == SegmentNodeConnectionEventType.OffsetChanged | t.Type == SegmentNodeConnectionEventType.TangentChanged)
                .Select(t => (offset: t.Offset, tangent: t.Tangent))
                .StartWith((offset: end.Offset, tangent: end.Tangent));


            _shapeChange = startPosition.CombineLatest(endPosition, startOffsetTangent, endOffsetTangent, (sPos, ePos, s, e) => (start: (position: sPos, offset: s.offset, tangent: s.tangent), end: (position: ePos, offset: e.offset, tangent: e.tangent)));


            if (sampler != null)
                _shapeChange = _shapeChange.Sample(sampler); // only once per frame


            _loftPath = new BehaviorSubjectValue<ILoftPath>(_shapeChange.Select(v => new BiArcLoftPath(v.start.position + v.start.offset, v.start.tangent, v.end.position + v.end.offset, v.end.tangent)));
            dispose.Add(_loftPath);
            dispose.Add(localEvents);



            var _width = description.Lanes.Sum(l => l.Width);
            var boundsSubject = _loftPath.Select(p => p.GetBounds(_width)).ToObservableValue();
            dispose.Add(boundsSubject);
            _bounds = boundsSubject;


            AIRoutes = createAiRoutes(_loftPath);
        }

        private IReadOnlyList<IAIRoute> createAiRoutes(BehaviorSubjectValue<ILoftPath> loftPath)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<SegmentEvent> observer)
        {
            return ((IObservable<SegmentEvent>)localEvents).Subscribe(observer);
        }

        public void Dispose()
        {
            RaiseEvent(SegmentEvent.Disconnect());
            localEvents.OnCompleted();
            dispose.Dispose();
        }

        private void RaiseEvent(SegmentEvent segmentEvent) => localEvents.OnNext(segmentEvent);

        public static Segment Create(Node startNode, Node endNode, SegmentDescription description)
        {
            var end = new SegmentNodeConnection(endNode);
            var start = new SegmentNodeConnection(startNode);
            var segment = new Segment(description, start, end);
            start.SetSegment(segment);
            end.SetSegment(segment);
            return segment;
        }
    }
}