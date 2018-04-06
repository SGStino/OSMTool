using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Simulation.Data;
using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Lofts;

namespace Simulation.Traffic.AI
{
    public class NodeAIPath : IAgentChainAIPath, IDisposable
    {
        private readonly CompositeDisposable dispose;
        private readonly SegmentAIPath source;
        private readonly SegmentAIPath destination;
        private readonly LinkedAgentChain agents;

        public NodeAIPath(SegmentAIPath source, SegmentAIPath destination, IObservableValue<ILoftPath> path)
        {
            this.dispose = new CompositeDisposable();
            this.source = source;
            this.destination = destination;
            this.agents = new LinkedAgentChain();
            this.LoftPath = path;
           var nextPaths = new BehaviorSubjectValue<IEnumerable<IAIPath>>(new[] { destination });
            NextPaths = nextPaths;
            dispose.Add(nextPaths);
            dispose.Add(Disposable.Create(agents.Clear));
        }

        public IObservableValue<ILoftPath> LoftPath { get; }

        public float SideOffsetStart => 0;

        public float SideOffsetEnd => 0;

        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => false;

        public float MaxSpeed => destination.MaxSpeed;

        public float AverageSpeed => (source.AverageSpeed + destination.AverageSpeed) / 2;

        public IObservableValue<IEnumerable<IAIPath>> NextPaths { get; }  

        public LaneType LaneType => destination.LaneType;

        public VehicleTypes VehicleTypes => destination.VehicleTypes;

        public float PathOffsetStart => 0.0f;

        public float PathOffsetEnd => 0.0f;

        public IObservableValue<ILoftPath> Path => LoftPath;

        public SegmentAIPath Source => source;

        public IAgentChain Agents => agents;

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextPaths.Value;

        public void Dispose() => dispose.Dispose();
    }
}
