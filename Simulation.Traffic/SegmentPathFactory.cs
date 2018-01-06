using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Lofts;
using UnityEngine;

namespace Simulation.Traffic
{
    public class SegmentPathFactory : IComponentValueFactory<ILoftPath, Segment>
    {
        public static SegmentPathFactory Default { get; } = new SegmentPathFactory();
        public Task<ILoftPath> Create(Segment owner, CancellationToken cancel)
        {
            var start = owner.Start;
            var end = owner.End;

            return Task.FromResult<ILoftPath>(new BiArcLoftPath(start.Node.Position, start.GetHeading(), end.Node.Position, end.GetHeading()));
        }
    }

    public static class SegmentExtensions
    {
        public static Vector3 GetHeading(this SegmentNodeConnection connnection) => connnection.Segment.Start == connnection ? -connnection.Tangent : connnection.Tangent;
        public static void SetHeading(this SegmentNodeConnection connnection, Vector3 heading) => connnection.Tangent = connnection.Segment.Start == connnection ? -heading : heading;
    }
}