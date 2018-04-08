using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public static class Directions3
    {
        public static Vector3 Up { get; } = new Vector3(0, 1, 0);
        public static Vector3 Forward { get; } = new Vector3(0, 0, 1);
        public static Vector3 Right { get; } = new Vector3(1, 0, 0);

        public static Vector3 Down { get; } = -Up;
        public static Vector3 Left { get; } = -Right;
        public static Vector3 Backward { get; } = -Forward;
    }
    public static class Directions2
    {
        public static Vector2 Up { get; } = new Vector2(0, 1 ); 
        public static Vector2 Right { get; } = new Vector2(1, 0);

        public static Vector2 Down { get; } = -Up;
        public static Vector2 Left { get; } = -Right; 
    }
}
