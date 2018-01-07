using Simulation.Traffic.Lofts;

namespace Simulation.Traffic
{
    public interface ISegmentPathFactory
    {
        ILoftPath Create(Segment owner);
    }
}