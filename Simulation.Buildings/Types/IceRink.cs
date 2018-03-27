using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class IceRink : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
            .SetCapacity(SportType.IceSkating, 16)
            .SetCapacity(SportType.IceHockey, 16)
            .SetCapacity(SportType.MartialArts, 16)
            .SetCapacity(SportType.Fitness, 32);

        public ISportProvider SportProvider => sports;
    }
}
