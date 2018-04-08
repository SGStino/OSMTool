using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using Simulation.Data.Trees;
using System;
using System.Numerics;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Reactive;
using System.Linq;
using Simulation.Data;
using System.Collections.Generic;
using Simulation.Data.Primitives;

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

    public interface ISegment : IDisposable, IBoundsObject2D
    {
        SegmentDescription Description { get; }
        //IObservable<ILoftPath> LoftPath { get; } 
        IObservableValue<ILoftPath> LoftPath { get; }

        IReadOnlyList<SegmentAIRoute> AIRoutes { get; }

        ISegmentNodeConnection Start { get; }
        ISegmentNodeConnection End { get; }
    }



    public class Segment : ISegment, IDisposable
    {

        private readonly BehaviorSubjectValue<ILoftPath> _loftPath;

        public IObservableValue<ILoftPath> LoftPath => _loftPath;

        private readonly CompositeDisposable dispose = new CompositeDisposable();
        private readonly IObservableValue<Rectangle> _bounds;


        public SegmentDescription Description { get; }


        public ISegmentNodeConnection Start { get; }
        public ISegmentNodeConnection End { get; }


        public IObservableValue<Rectangle> Bounds => _bounds;

        public IReadOnlyList<SegmentAIRoute> AIRoutes { get; }

        //private Rectangle getBounds()
        //{
        //    var a = Start.Node.Position;
        //    var b = End.Node.Position;

        //    var minX = MathF.Min(a.X, b.X);
        //    var minY = MathF.Min(a.Y, b.Y);

        //    var maxX = MathF.Max(a.X, b.X);
        //    var maxY = MathF.Max(a.Y, b.Y);

        //    return Rectangle.MinMaxRectangle(minX, minY, maxX, maxY);
        //}

        public Segment(SegmentDescription description, ISegmentNodeConnection start, ISegmentNodeConnection end, IObservable<Unit> sampler = null)
        {
            this.Description = description;
            Start = start;
            End = end;


            var startPosition = start
                .Node
                .Position;

            var endPosition = end
                .Node
                .Position;

            var startOffsetTangent = start
                .Offset;

            var endOffsetTangent = end
                .Offset;


            var shapeChange = startPosition.CombineLatest(endPosition, startOffsetTangent, endOffsetTangent, (sPos, ePos, s, e) => (start: (position: sPos, offset: s), end: (position: ePos, offset: e)));


            if (sampler != null)
                shapeChange = shapeChange.Sample(sampler); // only once per frame

             

            _loftPath = new BehaviorSubjectValue<ILoftPath>(shapeChange.Select(v => new BiArcLoftPath(v.start.offset.GetPosition(v.start.position), v.start.offset.Tangent, v.end.offset.GetPosition(v.end.position), v.end.offset.Tangent)));
            dispose.Add(_loftPath);



            var _width = description.Lanes.Sum(l => l.Width);
            var boundsSubject = _loftPath.Select(p => p.GetBounds(_width)).ToObservableValue();
            dispose.Add(boundsSubject);
            _bounds = boundsSubject;


            AIRoutes = createAiRoutes(description.SegmentFactory);
        }

        private SegmentAIRoute[] createAiRoutes(ISegmentAIPathsFactory factory) => factory.CreateRoutes(this);



        public void Dispose()
        {
            dispose.Dispose();
        }


        public static Segment Create(Node startNode, Node endNode, SegmentDescription description)
        {
            var a = Vector3.Normalize(endNode.Position.Value - startNode.Position.Value);
            var b = -a;

            var end = new SegmentNodeConnection(endNode, a);
            var start = new SegmentNodeConnection(startNode, b);
            var segment = new Segment(description, start, end);
            start.SetSegment(segment);
            end.SetSegment(segment);
            startNode.Connect(start);
            endNode.Connect(end);
            return segment;
        }
    }
}