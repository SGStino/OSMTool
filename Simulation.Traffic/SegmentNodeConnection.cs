using Simulation.Data;
using Simulation.Traffic.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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



    public struct ConnectionOffset
    {
        public ConnectionOffset(Vector3 offset, Vector3 tangent) : this()
        {
            Offset = offset;
            Tangent = tangent;
        }

        public Vector3 Offset { get; }
        public Vector3 Tangent { get; }
    }

    public interface ISegmentNodeConnection : IDisposable
    {
        ISegment Segment { get; }
        INode Node { get; }
        IObservableValue<ConnectionOffset> Offset { get; }
        IObservableValue<IReadOnlyList<NodeAIRoute>> OutgoingAIRoutes { get; }
        //IObservableValue<IReadOnlyList<IAIRoute>> IncomingAIRoutes { get; }
    }
    public class SegmentNodeConnection : ISegmentNodeConnection
    {
        private readonly CompositeDisposable disposable;

        public INode Node { get; }
        public ISegment Segment { get; private set; }

        private BehaviorSubjectValue<ConnectionOffset> offset = new BehaviorSubjectValue<ConnectionOffset>();



        public void Dispose()
        {
            disposable.Dispose();
        }

        void Move(ConnectionOffset transform) => offset.Value = transform;

        public IObservableValue<IReadOnlyList<NodeAIRoute>> OutgoingAIRoutes { get; }

        public IObservableValue<ConnectionOffset> Offset => offset;

        public SegmentNodeConnection(INode node, Vector3 tangent)
        {
            this.disposable = new CompositeDisposable();
            Node = node; 
            var routes = node.Connections.Where(n => n.Contains(this)).Select(n => updateAIRoutes(OutgoingAIRoutes?.Value, n)).ToObservableValue(new NodeAIRoute[0]);
            disposable.Add(routes);
            this.OutgoingAIRoutes = routes;
            disposable.Add(offset);
            offset.Value = new ConnectionOffset(new Vector3(0, 0, 5), tangent);
        }

        private IReadOnlyList<NodeAIRoute> updateAIRoutes(IReadOnlyList<NodeAIRoute> oldRoutes, IReadOnlyList<ISegmentNodeConnection> n)
        {
            INodeAIPathsFactory factory = Node.Description.Factory;

            var otherDestinations = new HashSet<ISegmentNodeConnection>(n.Where(i => i != this));

            var newRoutes = new List<NodeAIRoute>();

            if (oldRoutes != null)
                foreach (var routes in oldRoutes.GroupBy(k => k.Destination))
                {
                    if (otherDestinations.Remove(routes.Key))
                        newRoutes.AddRange(routes);
                    else
                        new CompositeDisposable(routes).Dispose();// this route is no longer connected!
                }

            foreach (var newDestination in otherDestinations)
            {
                newRoutes.AddRange(factory.CreateRoutes(this, newDestination));
            }


            return newRoutes;
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