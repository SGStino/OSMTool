using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI.Agents
{
    public struct PathDescription
    {
        public PathDescription(IAIPath Path, float Start, float End) : this()
        {
            this.Path = Path;
            this.Start = Start;
            this.End = End;
        }

        public IAIPath Path { get; }
        public float Start { get; }
        public float End { get; }
    } 
}
