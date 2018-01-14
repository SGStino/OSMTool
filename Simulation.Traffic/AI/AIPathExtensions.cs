using UnityEngine;

namespace Simulation.Traffic.AI
{
    public static class AIPathExtensions
    {
        public static float GetOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.LoftPath?.Length - path.PathOffsetEnd ?? 0;
            }
            else
                return path.PathOffsetStart;
        }

        public static float GetStartOffset(this IAIPath path) => GetOffset(path, end: false);
        public static float GetEndOffset(this IAIPath path) => GetOffset(path, end: true);

        public static float GetLength(this IAIPath path) => path.LoftPath?.Length - path.PathOffsetEnd - path.PathOffsetStart ?? 0;

        public static float GetOffsetPercentual(this IAIPath path, float n)
        {
            return Mathf.Lerp(path.GetStartOffset(), path.GetEndOffset(), n);
        }
        public static AISegmentNodeConnection GetStart(this SegmentAIPath path) => path.Reverse ? path.Segment.End : path.Segment.Start;
        public static AISegmentNodeConnection GetEnd(this SegmentAIPath path) => path.Reverse ? path.Segment.Start : path.Segment.End;

        public static Matrix4x4 GetStartTransform(this IAIPath path) => path.LoftPath?.GetTransform(path.GetStartOffset()) ?? Matrix4x4.identity;
        public static Matrix4x4 GetEndTransform(this IAIPath path) => path.LoftPath?.GetTransform(path.GetEndOffset()) ?? Matrix4x4.identity;
    }

}