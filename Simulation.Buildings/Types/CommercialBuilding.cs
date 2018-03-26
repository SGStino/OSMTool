using Simulation.Buildings.Types.Facilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class CommercialBuilding : Building, IFacilityProvider
    {
        public CommercialBuilding(params IFacility[] facilities)
        {
            Facilities = facilities;
        }

        public IEnumerable<IFacility> Facilities { get; }
    }
}
