using UnityEngine;

namespace Simulation.Traffic.AI
{
    public static class AIPathExtensions
    {
        public static float GetOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.Path?.Length - path.PathOffsetEnd ?? 0;
            }
            else
                return path.PathOffsetStart;
        }

        public static float GetStartOffset(this IAIPath path) => GetOffset(path, end: false);
        public static float GetEndOffset(this IAIPath path) => GetOffset(path, end: true);

        public static float GetOffsetPercentual(this IAIPath path, float n)
        {
            return Mathf.Lerp(path.GetStartOffset(), path.GetEndOffset(), n);
        }
    }

}