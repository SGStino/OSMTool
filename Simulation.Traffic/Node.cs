using Simulation.Traffic.AI;
using Simulation.Data.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Utilities;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive;
using Simulation.Data;
using Simulation.Data.Primitives;
using System.Reactive.Disposables;

namespace Simulation.Traffic
{


    public interface INode : IDisposable
    {
        NodeDescription Description { get; }
        IObservableValue<Vector3> Position { get; }
        IObservableValue<IReadOnlyList<ISegmentNodeConnection>> Connections { get; }
    }

    public class NodeDescription
    {
        public INodeAIPathsFactory Factory { get; set; }

        public static NodeDescription Intersection { get; } = new NodeDescription { Factory = NodeAIPathsFactory.Default };
    }

    public class Node : IBoundsObject2D, INode
    {

        private BehaviorSubjectValue<IReadOnlyList<ISegmentNodeConnection>> connections;

        private readonly IObservableValue<Rectangle> _bounds;
        private CompositeDisposable disposable = new CompositeDisposable();
        private BehaviorSubjectValue<Vector3> _position;
        private float radius = 6;

        public NodeDescription Description { get; }
        public IObservableValue<IReadOnlyList<ISegmentNodeConnection>> Connections => connections;

        public Node(Vector3 position, NodeDescription description, IObservable<Unit> sampler = null)
        {
            this._position = new BehaviorSubjectValue<Vector3>(position);
            Description = description;

            disposable.Add(this._position);

            var boundStream = Position.Select(t => getBounds(t)).ToObservableValue();
            disposable.Add(boundStream);
            this._bounds = boundStream;

            connections = new BehaviorSubjectValue<IReadOnlyList<ISegmentNodeConnection>>(new ISegmentNodeConnection[0]);
        }

        private Rectangle getBounds(Vector3 t)
        {
            return Rectangle.CornerSize(t.X - radius, t.Z - radius, radius * 2, radius * 2);
        }

        public void Dispose()
        {
            disposable.Dispose();
        }


        public void Connect(ISegmentNodeConnection connection)
        {
            lock (connections)
            {
                var oldItems = connections.Value;
                var newItems = new ISegmentNodeConnection[oldItems.Count + 1];
                for (int i = 0; i < oldItems.Count; i++)
                    newItems[i] = oldItems[i];
                newItems[oldItems.Count] = connection;

                Array.Sort(newItems, new AngleComparer());
                connections.Value = newItems;
            }
            //RaiseEvent(NodeChangeEvent.Connect(position));
        }

        public void Disconnect(ISegmentNodeConnection connection)
        {
            lock (connections)
            {
                var oldItems = connections.Value;
                var newItems = oldItems.Where(t => t != connection).ToArray();
                connections.Value = newItems;
            }
        }



        public IObservableValue<Vector3> Position => _position;

        public IObservableValue<Rectangle> Bounds => _bounds;

        public void Move(Vector3 destination)
        {
            _position.Value = destination;
        }


        public static Node CreateAt(float x, float z, NodeDescription description = null)
        {
            return new Node(new Vector3(x, 0, z), description ?? NodeDescription.Intersection);
        }
    }

    internal class AngleComparer : IComparer<ISegmentNodeConnection>
    {
        public int Compare(ISegmentNodeConnection x, ISegmentNodeConnection y)
        {
            var a = getAngle(x);
            var b = getAngle(y);
            return Comparer<float>.Default.Compare(a, b);
        }
        private float getAngle(ISegmentNodeConnection arg)
        {
            var tangent = arg.Offset.Value.Tangent;
            return MathF.Atan2(tangent.Z, tangent.X);
        }

    }
}
