using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.AI
{
    public static class RouteExtender
    {

        public static PathDescription[] Extend(this IEnumerable<IAIPath> paths, Matrix4x4 from, Matrix4x4 to)
        {
            var pointFrom = from.GetTranslate();
            var forwardFrom = from.GetForward();
            var pointTo = to.GetTranslate();
            var forwardto = to.GetForward();

            return paths.Extend(pointFrom, forwardFrom, pointTo, forwardto);
        }

        public static PathDescription[] Extend(this IEnumerable<IAIPath> paths, Vector3 pointFrom, Vector3 forwardFrom, Vector3 pointTo, Vector3 forwardTo)
        {
            var elements = paths.ToArray();
            var to = elements.Last().Extend(pointTo, forwardTo, false, out float pathPointTo);
            var from = elements.First().Extend(pointFrom, forwardFrom, true, out float pathPointFrom);

            var result = new PathDescription[elements.Length + 2];

            result[0] = to;

            for (int i = 0; i < elements.Length; i++)
            {
                var path = elements[i];

                float start = 0, end = 0;

                if (i == 0)
                    start = pathPointFrom;
                if (i == elements.Length - 1)
                    end = pathPointTo;

                result[i + 1] = new PathDescription(path, start, end);
            }

            result[result.Length - 1] = from;


            return result;
        }

        public static PathDescription Extend(this IAIPath path, Vector3 point, Vector3 forward, bool reverse, out float pathDistance)
        {
            Vector3 pointOnPath;
            Vector3 directionOnPath;
            if (path.Crosses(new Ray(point, forward), out float[] pathDistances, out float[] rayDistance))
            {
                pathDistance = pathDistances.Min();
                var transform = path.GetTransform(pathDistance);
                pointOnPath = transform.GetTranslate();
                directionOnPath = transform.GetForward();
            }
            else
            {
                var transformStart = path.GetStartTransform();
                var transformEnd = path.GetEndTransform();

                var startPoint = transformStart.GetTranslate();
                var endPoint = transformEnd.GetTranslate();

                var dStart = (startPoint - point).sqrMagnitude;
                var dEnd = (endPoint - point).sqrMagnitude;

                if (dStart < dEnd)
                {
                    pathDistance = 0;
                    pointOnPath = startPoint;
                    directionOnPath = transformStart.GetForward();
                }
                else
                {
                    pathDistance = path.GetLength();
                    pointOnPath = endPoint;
                    directionOnPath = transformEnd.GetForward();
                }
            }
            var loft = reverse
                ? new BiArcLoftPath(pointOnPath, directionOnPath, point, forward)
                : new BiArcLoftPath(point, forward, pointOnPath, directionOnPath);

            return new PathDescription(new AgentAIPath(loft), 0, 1);
        }

        public static bool Crosses(this IAIPath path, Ray ray, out float[] pathDistances, out float[] rayDistances)
        {
            if (path.LoftPath.Crosses(ray, out pathDistances, out rayDistances))
            {
                throw new NotImplementedException("Transform LoftDistances to PathDistances (reverse, offsets, ..)");
            } 
            return false;
        }
        public static bool Crosses(this ILoftPath loft, Ray ray, out float[] loftDistances, out float[] rayDistances)
        {
            var origin = ray.origin;
            var up = origin + Vector3.up;
            var forward = origin + ray.direction;

            var plane = new Plane(origin, up, forward);

            if(loft.Intersects(plane, out loftDistances))
            {
                rayDistances = new float[loftDistances.Length];
                throw new NotImplementedException("Transform LoftDistances to points to rayDistances");
            }
            loftDistances = rayDistances = new float[0];
            return false;
        }


    }
}
