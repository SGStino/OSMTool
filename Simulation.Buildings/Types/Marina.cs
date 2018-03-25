
using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class Marina : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new []{
            SportType.BoatRacing,
            SportType.Sailing,
            SportType.Jetski,
            SportType.WaterSki,
        };
    }
}
