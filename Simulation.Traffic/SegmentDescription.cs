namespace Simulation.Traffic
{
    public class SegmentDescription
    {
        public LaneDescription[] Lanes { get; set; }
        public ISegmentAIPathsFactory SegmentFactory { get; set; }
    }
}