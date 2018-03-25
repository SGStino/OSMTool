using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.People
{
    public interface IPerson
    { 
        IEnumerable<SportType> Sports { get; }
        IEnumerable<JobType> JobTypes { get; }
        IEnumerable<HobbyType> HobbyTypes { get; }
    }
}
