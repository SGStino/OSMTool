using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI.Agents
{
    public interface IAgent
    {
        float Progress { get; }
        float Length { get; }
    }
}
