
using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class Marina : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.BoatRacing, 32)
            .SetCapacity(SportType.Sailing, 50)
            .SetCapacity(SportType.Jetski, 50)
            .SetCapacity(SportType.WaterSki, 15);

        public ISportProvider SportProvider => sports;
    }
}
