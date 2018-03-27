using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class FitnessCenter : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
            .SetCapacity(SportType.Wrestling, 16)
            .SetCapacity(SportType.Boxing, 16)
            .SetCapacity(SportType.MartialArts, 16)
            .SetCapacity(SportType.Fitness, 32);

        public ISportProvider SportProvider => sports;
    }
}
