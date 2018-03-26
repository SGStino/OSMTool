using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class EquestrianCenter : Building, ISportLocation
    {
        public IEnumerable<SportType> AvailableSportTypes => new[] { SportType.Equestrian };
    }
}
