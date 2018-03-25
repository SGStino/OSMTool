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

        public IEnumerable<SportType> Sports { get; }
        public IEnumerable<JobType> JobTypes { get; }
        public IEnumerable<HobbyType> HobbyTypes { get; }
    }
}
