using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class SportsHall : Building, ISportLocation
    {
        public IEnumerable<SportType> AvailableSportTypes => new[]
        {
            SportType.Climbing,
            SportType.Fencing,
            SportType.MartialArts,
            SportType.Squash,
            SportType.Volleyball,
            SportType.Basketball,
            SportType.IndoorSoccer,
            SportType.Gymnastics,
        };
    }
}
