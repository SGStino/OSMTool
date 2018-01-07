using Simulation.Traffic.Lofts;
using System.Threading;
using System.Threading.Tasks;

namespace Simulation.Traffic
{
    internal class SegmentLoftPathComponent : RoadComponent<ILoftPath>
    {
        private readonly AISegment aISegment;
        private readonly ISegmentPathFactory pathFactory;

        public SegmentLoftPathComponent(AISegment aISegment, ISegmentPathFactory pathFactory)
        {
            this.aISegment = aISegment;
            this.pathFactory = pathFactory;
        }

        protected override Task<ILoftPath> GetValueAsync(CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested) return Task.FromCanceled<ILoftPath>(cancel);
            return Task.FromResult(pathFactory.Create(aISegment));
        }
    }
}