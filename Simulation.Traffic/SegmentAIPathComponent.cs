using Simulation.Traffic.AI;
using Simulation.Traffic.Lofts;
using System.Threading;
using System.Threading.Tasks;
using Simulation.Traffic.Utilities;

namespace Simulation.Traffic
{
    internal class SegmentAIPathComponent : RoadComponent<SegmentAIPath[]>
    {
        private readonly AISegment aISegment;
        private readonly ISegmentAIPathsFactory factory;

        public SegmentAIPathComponent(AISegment aISegment, ISegmentAIPathsFactory factory)
        {
            this.aISegment = aISegment;
            this.factory = factory;
        }

        protected override async Task<SegmentAIPath[]> GetValueAsync(CancellationToken cancel)
        {
            var cancelCombined = CancellationTokenSource.CreateLinkedTokenSource(cancel, aISegment.LoftPath.Result.Validity);
            var path = await aISegment.LoftPath.Result.Task;

            var paths = await factory.CreateAsync(aISegment, path, cancelCombined.Token);

            var compositeDisposable = new CompositeDisposable();
            compositeDisposable.AddRange(paths);
            compositeDisposable.Add(cancelCombined.Token.Register(compositeDisposable.Dispose));

            return paths;
        } 
    }
}