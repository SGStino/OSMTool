using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class FitnessCenter : Building, ISportLocation
    {
        public IEnumerable<SportType> AvailableSportTypes => new[]
        {
            SportType.Fitness,
            SportType.Boxing,
            SportType.MartialArts
        };
    }
}
