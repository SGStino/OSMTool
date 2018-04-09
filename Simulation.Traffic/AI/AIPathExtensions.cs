using Simulation.Data;
using Simulation.Traffic.Lofts;
using System;
using System.Reactive.Linq;
using System.Numerics;

namespace Simulation.Traffic.AI
{
    public static class AIPathExtensions
    {
        public static IObservable<Matrix4x4> ObserveTransform(this IAIPath aiPath, float distance, bool absolute = true)
        {
            var reverse = aiPath.Reverse;
            var path = aiPath.LoftPath;
            var offsets = aiPath.Offsets;




            return path.CombineLatest(offsets, (p, o) => GetTransform(distance, reverse, p, o, absolute));

            //path.Select(p => GetTransform(distance, reverse, p, offsets.Value));
            //offsets.Select(o => GetTransform(distance, reverse, path.Value, o));

            //return GetTransform(distance, reverse, path, offsets);
        }

        private static Matrix4x4 GetTransform(float distance, bool reverse, ILoftPath path, PathOffsets offsets, bool absolute)
        {
            var length = GetLength(path, offsets);

            float progress = distance;
            if (absolute) 
                progress = distance / length;

            if (reverse)
                progress = 1 - progress;

            distance = offsets.PathOffsetStart + progress * length;

            var maxDistance = path.Length;

            var t = progress;
            var lerp = MathF.Lerp(offsets.SideOffsetStart, offsets.SideOffsetEnd, t * t * t * (t * (6f * t - 15f) + 10f));

            var m = Matrix4x4.CreateTranslation(new Vector3(lerp, 0, 0));

            var result = path.GetTransform(distance) * m;

            if (reverse)
                return result * rotate;
            return result;
        }

        public static float GetLength(ILoftPath loftpath, PathOffsets offsets) =>
            loftpath?.Length - offsets.PathOffsetEnd - offsets.PathOffsetStart ?? 0;



        public static float GetPathOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.LoftPath.Value?.Length - path.Offsets.Value.PathOffsetEnd ?? 0;
            }
            else
                return path.Offsets.Value.PathOffsetStart;
        }

        public static float GetStartPathOffset(this IAIPath path) => GetPathOffset(path, end: false);
        public static float GetEndPathOffset(this IAIPath path) => GetPathOffset(path, end: true);


        public static float GetSideOffset(this IAIPath path, bool end)
        {
            if (path.Reverse ^ end)
            {
                return path.Offsets.Value.SideOffsetEnd;
            }
            else
                return path.Offsets.Value.SideOffsetStart;
        }

        public static float GetStartSideOffset(this IAIPath path) => GetSideOffset(path, end: false);
        public static float GetEndSideOffset(this IAIPath path) => GetSideOffset(path, end: true);


        public static float GetLength(this IAIPath path) => GetLength(path.LoftPath.Value, path.Offsets.Value);

        public static float GetOffsetPercentual(this IAIPath path, float n)
        {
            return MathF.Lerp(path.GetStartPathOffset(), path.GetEndPathOffset(), n);
        }
        //public static SegmentNodeConnection GetStart(this SegmentAIPath path) => path.Reverse ? path.Segment.End : path.Segment.Start;
        //public static SegmentNodeConnection GetEnd(this SegmentAIPath path) => path.Reverse ? path.Segment.Start : path.Segment.End;

        public static Matrix4x4 GetStartTransform(this IAIPath path) => path.GetTransform(0);
        public static Matrix4x4 GetEndTransform(this IAIPath path) => path.GetTransform(path.GetLength());

        public static Matrix4x4 rotate = new Matrix4x4(-1, 0, 0, 0, 0, 1, 0, 0, 0, 0, -1, 0, 0, 0, 0, 1);

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

            distance = path.Offsets.Value.PathOffsetStart + progress * length;

            var maxDistance = path.LoftPath.Value?.Length ?? 1;

            var t = progress;
            var lerp = MathF.Lerp(path.Offsets.Value.SideOffsetStart, path.Offsets.Value.SideOffsetEnd, t * t * t * (t * (6f * t - 15f) + 10f));

            var m = Matrix4x4.CreateTranslation(new Vector3(lerp, 0, 0));

            var result = path.LoftPath.Value.GetTransform(distance) * m;

            if (path.Reverse)
                return result * rotate;
            return result;
        }






        public static void SnapTo(this IAIPath path, Vector3 point, out float distance)
        {
            path.LoftPath.Value.SnapTo(point, out float loftDistance);

            float min, max;
            if (!path.Reverse)
            {
                min = path.Offsets.Value.PathOffsetStart;
                max = path.LoftPath.Value.Length - path.Offsets.Value.PathOffsetEnd;
            }
            else
            {
                loftDistance = path.LoftPath.Value.Length - loftDistance;
                max = path.LoftPath.Value.Length - path.Offsets.Value.PathOffsetStart;
                min = path.Offsets.Value.PathOffsetEnd;
            }
            distance = MathF.Clamp(loftDistance, min, max) - min;
        }
    }

}