using UnityEngine;

namespace Simulation.Traffic.AI
{
    public static class AIPathExtensions
    {
        public static float GetPathOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.LoftPath?.Length - path.PathOffsetEnd ?? 0;
            }
            else
                return path.PathOffsetStart;
        }

        public static float GetStartPathOffset(this IAIPath path) => GetPathOffset(path, end: false);
        public static float GetEndPathOffset(this IAIPath path) => GetPathOffset(path, end: true);


        public static float GetSideOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.SideOffsetEnd;
            }
            else
                return path.SideOffsetStart;
        }

        public static float GetStartSideOffset(this IAIPath path) => GetSideOffset(path, end: false);
        public static float GetEndSideOffset(this IAIPath path) => GetSideOffset(path, end: true);


        public static float GetLength(this IAIPath path) => path.LoftPath?.Length - path.PathOffsetEnd - path.PathOffsetStart ?? 0;

        public static float GetOffsetPercentual(this IAIPath path, float n)
        {
            return Mathf.Lerp(path.GetStartPathOffset(), path.GetEndPathOffset(), n);
        }
        public static AISegmentNodeConnection GetStart(this SegmentAIPath path) => path.Reverse ? path.Segment.End : path.Segment.Start;
        public static AISegmentNodeConnection GetEnd(this SegmentAIPath path) => path.Reverse ? path.Segment.Start : path.Segment.End;

        public static Matrix4x4 GetStartTransform(this IAIPath path) => path.GetTransform(0);
        public static Matrix4x4 GetEndTransform(this IAIPath path) => path.GetTransform(path.GetLength());

        public static Matrix4x4 rotate = new Matrix4x4(new Vector4(-1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, -1, 0), new Vector4(0, 0, 0, 1));

        public static float GetDistanceFromLoftPath(this IAIPath path, float loftpathDistance)
        {
            var nS = path.GetStartPathOffset();
            var nE = path.GetEndPathOffset();

            //n = (nE - nS) * t + nS;
            //(nE - nS) * t = n - nS          
            var t = (loftpathDistance - nS) / (nE - nS);


            return t * path.GetLength();
        }

        public static Matrix4x4 GetTransform(this IAIPath path, float distance)
        {
            var length = GetLength(path);
            var progress = distance / length;

            if (path.Reverse)
                progress = 1 - progress;

            distance = path.PathOffsetStart + progress * length;

            var maxDistance = path.LoftPath?.Length ?? 1;

            var t = progress;
            var lerp = Mathf.Lerp(path.SideOffsetStart, path.SideOffsetEnd, t * t * t * (t * (6f * t - 15f) + 10f));

            var m = Matrix4x4.Translate(new Vector3(lerp, 0, 0));

            var result = path.LoftPath.GetTransform(distance) * m;

            if (path.Reverse)
                return result * rotate;
            return result;
        }
    }

}