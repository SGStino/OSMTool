using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Lofts;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.AI
{
    public static class RouteExtender
    {

        public static PathDescription[] Extend(this IEnumerable<IAIPath> paths, Vector3 from, Vector3 to)
        {
            var pathArray = paths.ToArray();
            // from -> approach -> path[] -> depart -> to

            var first = paths.First();
            var end = paths.Last();

            first.SnapTo(from, out var closestFromDistance);
            end.SnapTo(to, out var closestToDistance);

            if (first == end)
            {
                if (closestToDistance < closestFromDistance)
                    closestToDistance = closestFromDistance = (closestFromDistance + closestToDistance) / 2;
            }


            var approachTransform = first.GetTransform(closestFromDistance);
            var departTransform = end.GetTransform(closestToDistance);



            var approachPoint = approachTransform.GetTranslate();
            var approachForward = approachTransform.GetForward();


            var departPoint = departTransform.GetTranslate();
            var departForward = departTransform.GetForward();


            var fromForward = Vector3.Normalize (approachPoint - from);
            var toForward = Vector3.Normalize(to - departPoint);

            var approachLoft = new BiArcLoftPath(from, -fromForward, approachPoint, -approachForward);
            var departLoft = new BiArcLoftPath(departPoint, -departForward, to, -toForward);

            var descriptions = new List<PathDescription>(pathArray.Length);


            //Debug.DrawLine(approachPoint, from, Color.cyan);
            //Debug.DrawLine(departPoint, to, Color.magenta);

            descriptions.Add(new PathDescription(new AgentAIPath(approachLoft), 0, 1));

            for (int i = 0; i < pathArray.Length; i++)
            {
                var path = pathArray[i];
                var s = 0f;
                var e = 1f;

                if (i == 0)
                    s = closestFromDistance / first.GetLength();
                if (i == pathArray.Length - 1)
                    e = closestToDistance / end.GetLength();

            

                descriptions.Add(new PathDescription(path, s, e));
            }

            descriptions.Add(new PathDescription(new AgentAIPath(departLoft), 0, 1));
            return descriptions.ToArray();

        }
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
                    start = pathPointFrom / path.GetLength();
                if (i == elements.Length - 1)
                    end = pathPointTo / path.GetLength();

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

                var dStart = (startPoint - point).LengthSquared();
                var dEnd = (endPoint - point).LengthSquared();

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
                ? new BiArcLoftPath(point, forward, pointOnPath, -directionOnPath)
                : new BiArcLoftPath(pointOnPath, -directionOnPath, point, forward);

            return new PathDescription(new AgentAIPath(loft), 0, 1);
        }

        public static bool Crosses(this IAIPath path, Ray ray, out float[] pathDistances, out float[] rayDistances)
        {
            if (path.LoftPath.Value.Crosses(ray, out pathDistances, out rayDistances))
            {
                for (int i = 0; i < pathDistances.Length; i++)
                    pathDistances[i] = loftToPathDistance(path, pathDistances[i]);

                return true;
            }
            return false;
        }

        private static float loftToPathDistance(IAIPath path, float v) => MathF.Lerp(path.GetStartPathOffset(), path.GetEndPathOffset(), v / path.GetLength());

        public static bool Crosses(this ILoftPath loft, Ray ray, out float[] loftDistances, out float[] rayDistances)
        {
            var origin = ray.Origin;
            var up = origin + Directions3.Up;
            var forward = origin + ray.Direction;
             
            var plane = Plane.CreateFromVertices(origin, up, forward);

            if (loft.Intersects(plane, out loftDistances))
            {
                rayDistances = new float[loftDistances.Length];

                for (int i = 0; i < loftDistances.Length; i++)
                {
                    var p = loft.GetTransformedPoint(loftDistances[i], Vector3.Zero);
                    rayDistances[i] = Vector3.Dot(p - ray.Origin, ray.Direction);
                }

                return true;
            }
            loftDistances = rayDistances = new float[0];
            return false;
        }


    }
}
