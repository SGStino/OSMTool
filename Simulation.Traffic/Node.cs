using Simulation.Traffic.AI;
using Simulation.Data.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Utilities;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;
using Simulation.Data;

namespace Simulation.Traffic
{
    public static class NodeExtensions
    {
        public static void UpdateOffsets(this Node node)
        {
            float minOffset = 1;
            int segmentCount = node.SegmentList.Count;

            if (segmentCount <= 0) return;
            if (segmentCount <= 1)
            {
                var radius = node.Segments.Max(l => l.Segment.GetWidth()) / 4;
                foreach (var seg in node.Segments)
                    seg.Offset = new Vector3(0, 0, Math.Max(minOffset, radius));
            }
            else
            {
                var segments = node.Segments.OrderBy(t => Math.Atan2(t.Tangent.z, t.Tangent.x)).ToArray();
                float[] offsets = new float[segments.Length];

                for (int curr = 0; curr < segmentCount; curr++)
                {
                    var prev = (curr - 1);
                    if (prev < 0) prev += segmentCount;

                    if (getOffsets(node, segments[prev], segments[curr], out var offsetA, out var offsetB))
                    {
                        offsets[curr] = Math.Max(minOffset, Mathf.Max(offsets[curr], offsetA));
                        offsets[prev] = Math.Max(minOffset, Mathf.Max(offsets[prev], offsetB));
                    }
                }

                for (int i = 0; i < segmentCount; i++)
                {
                    var offset = offsets[i];
                    if (!float.IsNaN(offset) && !float.IsInfinity(offset))
                    {
                        segments[i].Offset = new Vector3(0, 0, Math.Max(minOffset, Mathf.Min(50, offsets[i])));
                    }
                    else
                    {

                    }
                }
            }

        }

        private static bool getOffsets(Node node, SegmentNodeConnection con1, SegmentNodeConnection con2, out float offsetA, out float offsetB)
        {
            var wA = con1.Segment.GetWidth();
            var wB = con2.Segment.GetWidth();

            var o = node.Position.GetXZ();

            var dA = con1.Tangent.GetXZ();
            var dB = con2.Tangent.GetXZ();

            var dot = Vector3.Dot(dA, dB);
            if (dot < -0.75)
            {
                offsetA = float.PositiveInfinity;
                offsetB = float.PositiveInfinity;
                return false;
            }

            return VectorMath2D.IntersectsLineLine(o, dA, wA, o, dB, wB, out offsetA, out offsetB);

        }
    }

    //public class Node : Node
    //{
    //    public Node(Vector3 position, INodeAIPathsFactory aiFactory = null) : base(position)
    //    {
    //    }

    //    public void InvalidateAIPaths(Segment aISegment)
    //    {
    //    }

    //    public new IEnumerable<SegmentNodeConnection> Segments => base.Segments.OfType<SegmentNodeConnection>();

    //}

    public struct NodeChangeEvent
    {
        public NodeChangeEvent(NodeChangeEventType type, Vector3 position) : this()
        {
            Type = type;
            Position = position;
        }

        public NodeChangeEventType Type { get; }
        public Vector3 Position { get; }

        public static NodeChangeEvent Moved(Vector3 position) => new NodeChangeEvent(NodeChangeEventType.Moved, position);

        public static NodeChangeEvent Dispose(Vector3 position) => new NodeChangeEvent(NodeChangeEventType.Removed, position);
        public static NodeChangeEvent Connect(Vector3 position) => new NodeChangeEvent(NodeChangeEventType.Connected, position);
        public static NodeChangeEvent Disconnect(Vector3 position) => new NodeChangeEvent(NodeChangeEventType.Disconnected, position);
    }

    public enum NodeChangeEventType
    {
        Moved,
        Connected,
        Disconnected,
        Removed
    }

    public interface INode : IObservable<NodeChangeEvent>, IDisposable
    {
        Vector3 Position { get; }
    }

    public class Node : IBoundsObject2D, INode
    {
        private readonly Subject<NodeChangeEvent> localEvents = new Subject<NodeChangeEvent>();
        private readonly List<SegmentNodeConnection> segments = new List<SegmentNodeConnection>();
        private readonly IObservableValue<Rect> _bounds;
        private CompositeDisposable disposable = new CompositeDisposable();
        private Vector3 position;
        private float radius;

        public IReadOnlyList<SegmentNodeConnection> SegmentList => segments;


        public Node(Vector3 position, IObservable<Unit> sampler = null)
        {
            this.position = position;
            var positionStream = this.Where(t => t.Type == NodeChangeEventType.Moved).Select(t => t.Position).StartWith(position);
            if (sampler != null)
                positionStream = positionStream.Sample(sampler);

            var boundStream = positionStream.Select(t => getBounds(t)).ToObservableValue();
            disposable.Add(boundStream);
            disposable.Add(localEvents);
            this._bounds = boundStream;
        }

        private Rect getBounds(Vector3 t)
        {
            return new Rect(t.x - radius, t.z - radius, radius * 2, radius * 2);
        }

        public void Dispose()
        {
            RaiseEvent(NodeChangeEvent.Dispose(position));
            localEvents.OnCompleted();
            disposable.Dispose();
        }


        public void Connect(SegmentNodeConnection connection)
        {
            segments.Add(connection);
            updateOrder();
            RaiseEvent(NodeChangeEvent.Connect(position));
        }

        public void Disconnect(SegmentNodeConnection connection)
        {
            if (segments.Remove(connection))
                RaiseEvent(NodeChangeEvent.Disconnect(position));
        }

        private void RaiseEvent(NodeChangeEvent nodeChangeEvent) => localEvents.OnNext(nodeChangeEvent);

        public IEnumerable<SegmentNodeConnection> Segments => segments;


        public Vector3 Position
        {
            get => position;
            set
            {
                position = value;
                NotifyOfMovement();
            }
        }

        public IObservableValue<Rect> Bounds => _bounds;

        private void NotifyOfMovement()
        {
            RaiseEvent(NodeChangeEvent.Moved(position));
        }




        private void updateOrder()
        {
            this.segments.Sort(new AngleComparer(position));
        }



        public IDisposable Subscribe(IObserver<NodeChangeEvent> observer) => localEvents.Subscribe(observer);

        public static Node CreateAt(float x, float z)
        {
            return new Node(new Vector3(x, 0, z));
        }
    }

    internal class AngleComparer : IComparer<SegmentNodeConnection>
    {

        private Vector3 position;

        public AngleComparer(Vector3 position)
        {
            this.position = position;
        }

        public int Compare(SegmentNodeConnection x, SegmentNodeConnection y)
        {
            var a = getAngle(x);
            var b = getAngle(y);
            return Comparer<float>.Default.Compare(a, b);
        }
        private float getAngle(SegmentNodeConnection arg)
        {
            return Mathf.Atan2(arg.Tangent.z, arg.Tangent.x);
        }

    }
}
