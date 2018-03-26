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
            SportType.TableTennis,
        };
    }
    public class SportsCenter : Building, ISportLocation
    {
        public IEnumerable<SportType> AvailableSportTypes => new[]
        {
            SportType.TableTennis,
            SportType.Climbing,
            SportType.Fencing,
            SportType.MartialArts,
            SportType.Squash,
            SportType.Volleyball,
            SportType.Basketball,
            SportType.IndoorSoccer,
            SportType.Gymnastics,
            SportType.Soccer,
            SportType.Tennis,
            SportType.Boxing,
            SportType.Fitness,
            SportType.Rugby,
            SportType.Running
        };
    }

    public class CricketField : Building, ISportLocation
    {
        public IEnumerable<SportType> AvailableSportTypes => new[] { SportType.Cricket };
    }
}
