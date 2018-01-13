using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Simulation.Traffic.AI
{
    public class SegmentAIPathsFactory : ISegmentAIPathsFactory
    {
        public static SegmentAIPathsFactory Default { get; } = new SegmentAIPathsFactory();

        public SegmentAIRoute[] CreateRoutes(AISegment segment)
        {
            var lanes = segment.Description.Lanes;
            var paths = new SegmentAIPath[lanes.Length];

            float offset = -lanes.Sum(l => l.Width) / 2;

            for (byte i = 0; i < paths.Length; i++)
            {
                var path = Create(segment, lanes[i], ref offset, i);
                if (i > 0)
                    SegmentAIPath.ConnectParralel(paths[i - 1], path);
                paths[i] = path;
            }

            return paths.GroupBy(t => t.Reverse).Select(t => new SegmentAIRoute(segment, t.ToArray(), t.Key)).ToArray();
        }


        private SegmentAIPath Create(AISegment segment, LaneDescription laneDescription, ref float offset, byte id)
        {
            var path = new SegmentAIPath(segment, laneDescription, offset + laneDescription.Width / 2, id);
            offset += path.Lane.Width;
            return path;
        }
    }
}
