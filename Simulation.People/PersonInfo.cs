using System;
using System.Collections.Generic;
using System.Text;
using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;

namespace Simulation.People
{
    public class PersonInfo : IPerson
    { 

        public IReadOnlyCollection<SportType> Sports { get; }
        public IReadOnlyCollection<JobType> JobTypes { get; }
        public IReadOnlyCollection<HobbyType> HobbyTypes { get; }
    }
}
