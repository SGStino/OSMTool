using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Traffic.AI.Agents;
namespace Simulation.Traffic.AI
{
    public class SegmentAIPath : IAgentChainAIPath, IDisposable
    {
        public byte ID { get; }
        private readonly AISegment segment;
        private readonly LaneDescription laneDescription;
        private readonly float offset;
        private readonly LinkedAgentChain agents;

        public SegmentAIPath( AISegment segment, LaneDescription laneDescription, float offset, byte id)
        { 
            this.segment = segment;
            this.laneDescription = laneDescription;
            this.offset = offset;

            this.agents = new LinkedAgentChain();
        }


        public IAIPath LeftParralel { get; private set; }

        public IAIPath RightParralel { get; private set; }

        public bool Reverse => laneDescription.Reverse;

        public IEnumerable<IAIPath> NextPaths 
            => this.GetEnd()
            .AIRoutes
            .SelectMany(t => t.Paths)
            .Where(t => t.Source == this);

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

        public ILoftPath LoftPath => Segment.LoftPath;

        public float SideOffsetStart => offset;

        public float SideOffsetEnd => offset;

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public AISegment Segment => segment;

         IEnumerable<IAIGraphNode> IAIGraphNode.NextNodes => NextPaths;

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
