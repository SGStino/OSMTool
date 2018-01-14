using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.AI.Agents
{
    public class Agent
    {
        private Sequence<IAIRoute> routeSequence;

        public void Update(float dt)
        {
        }

        private bool GoToNextPath()
        {
            return routeSequence?.Next() ?? false;
        }

        //public Matrix4x4 GetTransform() => currentPath.LoftPath.GetTransform(PathProgress);


        public void SetRoute(IAIRoute[] route, int index = 0)
        {
            this.routeSequence = new Sequence<IAIRoute>(route);
        }
    }
}
