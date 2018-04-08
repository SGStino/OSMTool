using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Simulation.Data.Primitives;
namespace Simulation.Data.Trees
{
    public static class QuadTreeUtils
    {
        public static bool Encapsulates(this Rectangle maxBounds, Rectangle bounds)
        {
            if (bounds.Min.X <= maxBounds.Min.X) return false;
            if (bounds.Min.Y <= maxBounds.Min.Y) return false;
            if (bounds.Max.X >= maxBounds.Max.X) return false;
            if (bounds.Max.Y >= maxBounds.Max.Y) return false;
            return true;
        }

        public static Rectangle[] DevideQuads(Rectangle bounds)
        {
            var x0 = bounds.Min.X;
            var y0 = bounds.Min.Y;
            var x2 = bounds.Max.X;
            var y2 = bounds.Max.Y;
            var x1 = (x2 + x0) / 2;
            var y1 = (y2 + y0) / 2;


            var yCoord = new float[,] { { y0, y1 }, { y1, y2 } };
            var xCoord = new float[,] { { x0, x1 }, { x1, x2 } };

            var children = new Rectangle[4];

            for (int i = 0; i < 4; i++)
            {
                var x = i % 2;
                var y = i / 2;

                children[i] = Rectangle.MinMaxRectangle(xCoord[x, 0], yCoord[y, 0], xCoord[x, 1], yCoord[y, 1]);
            }
            return children;
        }

        internal static Rectangle Combine(this Rectangle source, Rectangle target)
        { 
            Vector2 Min, Max;
            Min.X = Math.Min(source.Min.X, target.Min.X);
            Min.Y = Math.Min(source.Min.Y, target.Min.Y);
            Max.X = Math.Max(source.Max.X, target.Max.X);
            Max.Y = Math.Max(source.Max.Y, target.Max.Y);

            return Rectangle.MinMaxRectangle(Min, Max);
        }



        public static int GetQuadIndex(Rectangle node, Vector2 target)
        {
            var max = node.Center;
            var xPos = max.X < target.X;
            var yPos = max.Y < target.Y;
            return (xPos ? 1 : 0) + (yPos ? 2 : 0);
        }

        public static Rectangle GrowQuad(Rectangle initial, Vector2 target, out int targetIndex, out int sourceIndex)
        {


            var xPos = initial.Center.X < target.X;
            var yPos = initial.Center.Y < target.Y;


            Vector2 min;
            Vector2 max;


            GetCoordinates(initial.Min.X, initial.Max.X, xPos, out min.X, out max.X);
            GetCoordinates(initial.Min.Y, initial.Max.Y, yPos, out min.Y, out max.Y);

            targetIndex = (xPos ? 1 : 0) + (yPos ? 2 : 0);
            sourceIndex = (xPos ? 0 : 1) + (yPos ? 0 : 2);
            return Rectangle.MinMaxRectangle(min.X, min.Y, max.X, max.Y);
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
