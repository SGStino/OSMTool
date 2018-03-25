using Simulation.Leisure.Sports;
using System.Collections.Generic;

namespace Simulation.People.Buildings.Types
{
    public class Marina : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new []{
            SportType.BoatRacing,
            SportType.Sailing,
            SportType.Jetski,
            SportType.WaterSki,
        };
    }
}
