using Simulation.Buildings;
using Simulation.Leisure.Sports; 
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class WaterSportCenter : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.Sailing, 32)
            .SetCapacity(SportType.Rowing, 32);

        public ISportProvider SportProvider => sports; 
    }
}
