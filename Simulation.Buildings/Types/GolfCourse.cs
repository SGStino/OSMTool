using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class GolfCourse : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.Golf, 32);

        public ISportProvider SportProvider => sports;
    }
}
