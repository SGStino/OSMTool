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
        IReadOnlyCollection<SportType> Sports { get; }
        IReadOnlyCollection<JobType> JobTypes { get; }
        IReadOnlyCollection<HobbyType> HobbyTypes { get; }
    }
}
