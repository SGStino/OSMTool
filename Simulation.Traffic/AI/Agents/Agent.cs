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
        public int PathIndex { get; private set; }

        public IAIPath[] Route { get; }

        public float PathProgress { get; private set; }

        public float Speed { get; private set; } = 1;

        public IAIPath CurrentPath => Route[PathIndex];

        public bool IsValid => Route != null && PathIndex >= 0 && PathIndex < Route.Length;

        public void Update(float dt)
        {
            if (IsValid)
            {
                PathProgress += Speed * dt;

                var remaining = CurrentPath.Path.Length - PathProgress;

                if (remaining < 0)
                {
                    PathProgress = -remaining;
                    PathIndex++;
                    if (IsValid)
                        Speed = CurrentPath.MaxSpeed;
                }
            }
        }

        public Matrix4x4 GetTransform() => CurrentPath.Path.GetTransform(PathProgress);


        public void SetRoute(IAIPath[] route)
        {
            PathProgress = 0;
            PathIndex = 0;
            if (IsValid)
                Speed = CurrentPath.MaxSpeed;
        }
    }
}
