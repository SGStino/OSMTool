using Simulation.Buildings;
using Simulation.Leisure.Sports; 
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
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
