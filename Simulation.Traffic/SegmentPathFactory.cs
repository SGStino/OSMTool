using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using UnityEngine;

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
        public static Vector3 GetHeading(this SegmentNodeConnection connnection) => connnection.Segment.Start == connnection ? -connnection.Tangent : connnection.Tangent;
        public static void SetHeading(this SegmentNodeConnection connnection, Vector3 heading) => connnection.Tangent = connnection.Segment.Start == connnection ? -heading : heading;


        public static Vector3 GetPosition(this SegmentNodeConnection con)
        {
            var tangent = con.Tangent;
            return con.Node.Position
            + tangent * con.Offset.z
            + Vector3.Cross(tangent, Vector3.up) * con.Offset.x
            + Vector3.up * con.Offset.y;
        }
    }
}