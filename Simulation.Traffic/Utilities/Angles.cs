using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Utilities
{

    public static class Angles
    {
        public static bool IsAngleBetweenDegrees(float angle, float min, float max)
        {
            angle = angle - min;
            max = max - min;

            bool result = false;
            if (max < 0)
            {
                if (angle < 0 && angle > max)
                    result = true;
            }
            else
            {
                if (angle > 0 && angle < max)
                    result = true;
            }
            return result;


        }
    }
}
