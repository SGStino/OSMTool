using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types.Facilities
{
    public class FitnessStudio : IFacility, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new[] { SportType.Fitness, SportType.Yoga };
    }
    public class BoxingClub : IFacility, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new[] { SportType.Boxing, SportType.Wrestling };
    }
}
