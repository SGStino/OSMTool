using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Trees
{
    public static class QuadTreeUtils
    {
        public static bool Encapsulates(this Rect maxBounds, Rect bounds)
        {
            if (bounds.xMin <= maxBounds.xMin) return false;
            if (bounds.yMin <= maxBounds.yMin) return false;
            if (bounds.xMax >= maxBounds.xMax) return false;
            if (bounds.yMax >= maxBounds.yMax) return false;
            return true;
        }

        public static Rect[] DevideQuads(Rect bounds)
        {
            var x0 = bounds.min.x;
            var y0 = bounds.min.y;
            var x2 = bounds.max.x;
            var y2 = bounds.max.y;
            var x1 = (x2 + x0) / 2;
            var y1 = (y2 + y0) / 2;


            var yCoord = new float[,] { { y0, y1 }, { y1, y2 } };
            var xCoord = new float[,] { { x0, x1 }, { x1, x2 } };

            var children = new Rect[4];

            for (int i = 0; i < 4; i++)
            {
                var x = i % 2;
                var y = i / 2;

                children[i] = Rect.MinMaxRect(xCoord[x, 0], yCoord[y, 0], xCoord[x, 1], yCoord[y, 1]);
            }
            return children;
        }

        internal static Rect Combine(this Rect source, Rect target)
        {
            var xMin = Math.Min(source.xMin, target.xMin);
            var yMin = Math.Min(source.yMin, target.yMin);
            var xMax = Math.Max(source.xMax, target.xMax);
            var yMax = Math.Max(source.yMax, target.yMax);

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        internal static bool Overlaps(this Rect bounds, Vector2 point, float radius)
        {
            var pX = point.x - Mathf.Clamp(point.x, bounds.xMin, bounds.xMax);
            var pY = point.y - Mathf.Clamp(point.y, bounds.yMin, bounds.yMax);

            var r2 = radius * radius;

            return (pX * pX + pY * pY) <= r2;
        }

        public static int GetQuadIndex(Rect node, Vector2 target)
        {
            var max = node.center;
            var xPos = max.x < target.x;
            var yPos = max.y < target.y;
            return (xPos ? 1 : 0) + (yPos ? 2 : 0);
        }

        public static Rect GrowQuad(Rect initial, Vector2 target, out int targetIndex, out int sourceIndex)
        {


            var xPos = initial.center.x < target.x;
            var yPos = initial.center.y < target.y;


            Vector2 min;
            Vector2 max;


            GetCoordinates(initial.xMin, initial.xMax, xPos, out min.x, out max.x);
            GetCoordinates(initial.yMin, initial.yMax, yPos, out min.y, out max.y);

            targetIndex = (xPos ? 1 : 0) + (yPos ? 2 : 0);
            sourceIndex = (xPos ? 0 : 1) + (yPos ? 0 : 2);
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        private static void GetCoordinates(float min, float max, bool dir, out float left, out float right)
        {
            var size = (max - min);
            if (dir)
            {
                left = min;
                right = max + size;
            }
            else
            {
                left = min - size;
                right = max;
            }

        }

    }
}
