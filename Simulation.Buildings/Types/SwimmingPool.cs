using Simulation.Buildings;
using Simulation.Leisure.Sports;
using System.Collections.Generic;

namespace Simulation.Buildings.Types
{
    public class SwimmingPool : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.WaterPolo, 32)
            .SetCapacity(SportType.Swimming, 32)
            .SetCapacity(SportType.WaterAerobics, 32);

        public ISportProvider SportProvider => sports;
    }
}
