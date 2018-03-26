using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Buildings.Types.Facilities
{
    public interface IFacilityProvider
    {
        IEnumerable<IFacility> Facilities { get; }
    }
}
