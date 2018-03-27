using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class EquestrianCenter : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
            .SetCapacity(SportType.Equestrian, 16);

        public ISportProvider SportProvider => sports;
    }
}
