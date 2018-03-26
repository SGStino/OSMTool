using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class IceRink : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new[] { SportType.IceSkating, SportType.IceHockey };
    }
}
