using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public interface ILoftShape
    {
        LoftVertex[] Vertices { get; } 
        int[] Indices { get; }
        float VScale { get; }
    }
}