using Simulation.Leisure.Sports;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class GolfCourse : Building, ISportLocation
    {
        IEnumerable<SportType> ISportLocation.AvailableSportTypes => new[] { SportType.Golf };
    }
}
