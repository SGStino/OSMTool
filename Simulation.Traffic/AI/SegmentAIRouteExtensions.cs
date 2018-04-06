namespace Simulation.Traffic.AI
{
    public static class SegmentAIRouteExtensions
    {
        public static ISegmentNodeConnection GetStart(this SegmentAIRoute route) => route.Reverse ? route.Segment.End : route.Segment.Start;

        public static ISegmentNodeConnection GetEnd(this SegmentAIRoute route) => route.Reverse ? route.Segment.Start : route.Segment.End;

        public static ISegmentNodeConnection GetStart(this SegmentAIPath route) => route.Reverse ? route.Segment.End : route.Segment.Start;

        public static ISegmentNodeConnection GetEnd(this SegmentAIPath route) => route.Reverse ? route.Segment.Start : route.Segment.End;

    }
}
