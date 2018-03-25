using Simulation.Leisure.Sports;
using System.Collections.Generic;

namespace Simulation.People.Buildings.Types
{
    public class WaterSportCenter : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new[]
        {
            SportType.Sailing,
            SportType.Rowing,
        };
    }
}
