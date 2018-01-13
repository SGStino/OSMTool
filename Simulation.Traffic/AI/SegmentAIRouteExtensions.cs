namespace Simulation.Traffic.AI
{
    public static class SegmentAIRouteExtensions
    {
        public static AISegmentNodeConnection GetStart(this SegmentAIRoute route) => route.Reverse ? route.Segment.End : route.Segment.Start;

        public static AISegmentNodeConnection GetEnd(this SegmentAIRoute route) => route.Reverse ? route.Segment.Start : route.Segment.End;
    }
}
