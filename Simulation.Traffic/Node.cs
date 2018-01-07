using Simulation.Traffic.AI;
using Simulation.Traffic.Trees;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Utilities;

namespace Simulation.Traffic
{
    public class AINode : Node
    {
        public IRoadComponent<NodeAIPath[]> AIPaths { get; }

        public AINode(Vector3 position, RoadManager manager, INodeAIPathFactory aiFactory = null) : base(position, manager)
        {
            AIPaths = new NodeAIPathComponent(this, aiFactory ?? NodeAIPathsFactory.Default);

            Invalidated += AIPaths.Invalidate;

            AIPaths.Invalidated += NotifyOfAIPathInvalidation;
        }

        private void NotifyOfAIPathInvalidation()
        {
            //TODO: ask roadmanager for an update
        }

        public void InvalidateAIPaths(AISegment aISegment)
        {
            AIPaths.Invalidate();
        }
    }

    internal class NodeAIPathComponent : RoadComponent<NodeAIPath[]>
    {
        private AINode aINode;
        private INodeAIPathFactory factory;

        public NodeAIPathComponent(AINode aINode, INodeAIPathFactory factory)
        {
            this.aINode = aINode;
            this.factory = factory;
        }

        protected override async Task<NodeAIPath[]> GetValueAsync(CancellationToken cancel)
        {
            var paths = await CreatePaths(cancel);

            var compositeDisposable = new CompositeDisposable();
            compositeDisposable.AddRange(paths);
            compositeDisposable.Add(cancel.Register(compositeDisposable.Dispose));

            return paths;
        }
        private Task<NodeAIPath[]> CreatePaths(CancellationToken cancel)
        {
            return factory.CreateAsync(aINode, cancel);
        }
    }

    public class Node : IBoundsObject2D
    {
        public event Action Invalidated;
        internal IList<SegmentNodeConnection> SegmentList => segments;

        private List<SegmentNodeConnection> segments = new List<SegmentNodeConnection>();
        public RoadManager Manager { get; internal set; }

        private Vector3 position;

        public Node(Vector3 position, RoadManager manager)
        {
            this.Manager = manager;
            this.position = position;
        }


        public IEnumerable<SegmentNodeConnection> Segments => segments;

        public Rect Bounds
        {
            get
            {
                float radius = 1;

                Vector2 min, max;
                min.x = Position.x - radius;
                max.x = Position.x + radius;

                min.y = Position.y - radius;
                max.y = Position.y + radius;

                return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
            }
        }

        public Vector3 Position
        {
            get => position; set
            {
                position = value;
                NotifyOfMovement();
            }
        }

        internal void NotifyOfCreation()
        {
            OnCreated();
        }

        internal void NotifyOfMovement()
        {
            Invalidate();
            OnMoved();
            Manager.BoundsChanged(this);
            foreach (var con in segments)
                con.NotifyOfMovement();
        }

        private void Invalidate()
        {
            Invalidated?.Invoke();
        }

        protected virtual void OnMoved()
        {
        }


        protected virtual void OnCreated()
        {
        }

        internal void NotifyOfTangentChanged(SegmentNodeConnection segmentNodeConnection)
        {
            updateOrder();
            OnTangentChanged(segmentNodeConnection);
            Invalidate();
        }

        protected virtual void OnTangentChanged(SegmentNodeConnection segmentNodeConnection)
        {
        }

        private void updateOrder()
        {
            var segments = this.segments.OrderBy(getAngle).ToArray();
            for (int i = 0; i < segments.Length; i++)
            {
                var current = segments[i];
                var next = segments[(i + 1) % segments.Length];

                next.Left = current;
                current.Right = next;
            }
        }

        private float getAngle(SegmentNodeConnection arg)
        {
            return Mathf.Atan2(arg.Tangent.z, arg.Tangent.x);
        }

        /// <summary>
        /// Gets called by Manager
        /// </summary>
        internal void NotifyOfRemoval()
        {
            OnRemoved();
        }

        protected virtual void OnRemoved()
        {
        }

        internal void NotifiyOfDisconnect(SegmentNodeConnection connection)
        {
            updateOrder();
            OnDisconnected(connection);
            Invalidate();
        }

        protected virtual void OnDisconnected(SegmentNodeConnection connection)
        {
        }

        internal void NotifyOfConnection(SegmentNodeConnection connection)
        {
            updateOrder();
            OnConnected(connection);
            Invalidate();
        }

        protected virtual void OnConnected(SegmentNodeConnection connection)
        {
        }
    }
}
