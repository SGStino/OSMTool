using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class SportsHall : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
           .SetCapacity(SportType.Climbing, 16)
           .SetCapacity(SportType.Fencing, 16)
           .SetCapacity(SportType.MartialArts, 16)
           .SetCapacity(SportType.Squash, 32)
           .SetCapacity(SportType.Volleyball, 32)
           .SetCapacity(SportType.Basketball, 32)
           .SetCapacity(SportType.IndoorSoccer, 32)
           .SetCapacity(SportType.Gymnastics, 32)
           .SetCapacity(SportType.TableTennis, 32)
           .SetCapacity(SportType.Squash, 32);

        public ISportProvider SportProvider => sports;
    }
    public class SportsCenter : Building, ISportLocation
    {

        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.TableTennis, 32)
             .SetCapacity(SportType.Climbing, 32)
             .SetCapacity(SportType.Fencing, 32)
             .SetCapacity(SportType.MartialArts, 32)
             .SetCapacity(SportType.Squash, 32)
             .SetCapacity(SportType.Volleyball, 32)
             .SetCapacity(SportType.Basketball, 32)
             .SetCapacity(SportType.IndoorSoccer, 32)
             .SetCapacity(SportType.Gymnastics, 32)
             .SetCapacity(SportType.Soccer, 32)
             .SetCapacity(SportType.Tennis, 32)
             .SetCapacity(SportType.Boxing, 32)
             .SetCapacity(SportType.Fitness, 32)
             .SetCapacity(SportType.Rugby, 32)
             .SetCapacity(SportType.Running, 32);

        public ISportProvider SportProvider => sports;
    }

    public class CricketField : Building, ISportLocation
    {
        private SportsRegistrationCollection sports = new SportsRegistrationCollection()
             .SetCapacity(SportType.Cricket, 32);

        public ISportProvider SportProvider => sports;
    }
}
