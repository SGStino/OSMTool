using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using UnityEngine;
using System.Linq;

namespace Simulation.Traffic
{
    public class SegmentPathFactory : ISegmentPathFactory
    {
        public static SegmentPathFactory Default { get; } = new SegmentPathFactory();
        public ILoftPath Create(Segment owner)
        {
            var start = owner.Start;
            var end = owner.End;
            if (start == null || end == null) return null;
            return new BiArcLoftPath(start.GetPosition(), start.GetHeading(), end.GetPosition(), end.GetHeading());
        }
    }

    public static class SegmentExtensions
    {
        public static Vector3 GetHeading(this ISegmentNodeConnection connnection) => connnection.Segment.Start == connnection ? -connnection.Tangent : connnection.Tangent;
        public static void SetHeading(this ISegmentNodeConnection connnection, Vector3 heading) => connnection.Tangent = connnection.Segment.Start == connnection ? -heading : heading;

        public static float GetWidth(this ISegment segment)
        {
            return segment.Description.Lanes.Sum(l => l.Width);
        }

        public static Vector3 GetPosition(this ISegmentNodeConnection con)
        {
            return GetPosition(con, Vector3.zero);
        }

        public static Vector3 GetPosition(this ISegmentNodeConnection con, Vector3 additionalOffset)
        {
            var off = con.Offset + additionalOffset;
            var tangent = con.Tangent;
            return con.Node.Position
            + tangent * off.z
            + Vector3.Cross(tangent, Vector3.up) * off.x
            + Vector3.up * off.y;
        }
    }
}