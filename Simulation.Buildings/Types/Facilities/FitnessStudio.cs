using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types.Facilities
{
    public class FitnessStudio : IFacility, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
            .SetCapacity(SportType.Fitness, 64)
            .SetCapacity(SportType.Yoga, 48);

        public ISportProvider SportProvider => sports;
    }
    public class BoxingClub : IFacility, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
            .SetCapacity(SportType.Wrestling, 16)
            .SetCapacity(SportType.Boxing, 16);

        public ISportProvider SportProvider => sports; 
    }
}
