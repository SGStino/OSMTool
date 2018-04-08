using System.Numerics;

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
                X = d.X,
                Y = d.Y,
                Z = d.Z
            };
        }
        public static implicit operator Vector3(Vector3D d)
        {
            return new Vector3
            {
                X = d.X,
                Y = d.Y,
                Z = d.Z
            };
        }
    }
}