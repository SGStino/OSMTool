using Simulation.Buildings;
using Simulation.Leisure.Sports;
using System.Collections.Generic;

namespace Simulation.Buildings.Types
{
    public class SwimmingPool : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new []
        {
            SportType.Swimming,
            SportType.WaterPolo,
            SportType.WaterAerobics
        };
    }
}
