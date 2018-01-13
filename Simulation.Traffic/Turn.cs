using System;

namespace Simulation.Traffic
{
    [Flags]
    public enum Turn
    {
        None = 0,
        SharpLeft = 1,
        Left = 2,
        SlightLeft = 4,
        MergeLeft = 8,
        Through = 16, 
        MergeRight = 32,
        SlightRight = 64,
        Right = 128,
        SharpRight = 256,
        Reverse = 512
    }
}