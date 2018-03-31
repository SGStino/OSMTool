using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Leisure.Sports
{
    public interface ISportLocation
    {
        ISportProvider SportProvider { get; }
    }

    public interface ISportProvider : IObservable<SportSubscriptionChangeEvent>
    {
        IEnumerable<SportType> AvailableSportTypes { get; }
        int GetOccupancy(SportType type);
        int GetCapacity(SportType type);
        bool Join(SportType type);
        bool Leave(SportType type);
    }
}
