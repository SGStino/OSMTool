using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Traffic.AI.Agents;
using Simulation.Data;
using System.Reactive.Linq;

namespace Simulation.Traffic.AI
{
    public class SegmentAIPath : IAgentChainAIPath, IDisposable
    {
        public byte ID { get; }
        private readonly Segment segment;
        private readonly LaneDescription laneDescription;
        private readonly float offset;
        private readonly LinkedAgentChain agents;

        public SegmentAIPath(Segment segment, LaneDescription laneDescription, float offset, byte id)
        {
            this.segment = segment;
            this.laneDescription = laneDescription;
            this.offset = offset;

            this.agents = new LinkedAgentChain();

            var nextPaths = this.GetEnd().OutgoingAIRoutes.Select(routes => routes.SelectMany(route => route.Paths)).ToObservableValue();
            NextPaths = nextPaths;
        }


        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => laneDescription.Reverse;

        public IObservableValue<IEnumerable<IAIPath>> NextPaths { get; }

        public LaneType LaneType => laneDescription.LaneType;

        public VehicleTypes VehicleTypes => laneDescription.VehicleTypes;

        public float MaxSpeed => laneDescription.MaxSpeed;

        public float AverageSpeed => Agents // TODO: PREF: cache?
            .IterateBackward()
            .Take(10)
            .Select(t => t.Agent.Speed)
            .DefaultIfEmpty(MaxSpeed)
            .Average();

        public LaneDescription Lane => laneDescription;

        public IObservableValue<ILoftPath> LoftPath => Segment.LoftPath;

        public float SideOffsetStart => offset;

        public float SideOffsetEnd => offset;

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public Segment Segment => segment;

        IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextPaths.Value;

        public IAgentChain Agents => agents;

        public static void ConnectParralel(SegmentAIPath left, SegmentAIPath right)
        {
            left.RightParralel = right;
            right.LeftParralel = left;
        }

        public void Dispose()
        {
            // TODO: notify agents of path removal
            agents.Clear();
        }
    }
}
