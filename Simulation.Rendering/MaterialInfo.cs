using System;

namespace Simulation.Rendering
{
    public struct MaterialInfo : IEquatable<MaterialInfo>
    {
        public MaterialInfo(int materialIndex) : this()
        {
            MaterialIndex = materialIndex;
        }

        public int MaterialIndex { get; }

        public override int GetHashCode()
        {
            return MaterialIndex.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is MaterialInfo other)
                return Equals(other);
            return false;
        }

        public bool Equals(MaterialInfo other)
        {
            return other.MaterialIndex == MaterialIndex;
        }
    }
}
