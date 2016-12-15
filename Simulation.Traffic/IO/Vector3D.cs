using UnityEngine;

namespace Simulation.Traffic.IO
{
    internal struct Vector3D
    {
        public float X;
        public float Y;
        public float Z;

        public static implicit operator Vector3D(Vector3 d)
        {
            return new Vector3D
            {
                X = d.x,
                Y = d.y,
                Z = d.z
            };
        }
        public static implicit operator Vector3(Vector3D d)
        {
            return new Vector3
            {
                x = d.X,
                y = d.Y,
                z = d.Z
            };
        }
    }
}