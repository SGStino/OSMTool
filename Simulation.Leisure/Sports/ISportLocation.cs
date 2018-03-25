using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Leisure.Sports
{
    public interface ISportLocation
    {
        IEnumerable<SportType> AvailableSportTypes { get; }
    }
}
