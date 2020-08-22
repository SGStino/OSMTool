using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static partial class MathF
    {
        public const float PI = (float)Math.PI;
        public const float TwoPI = (float)Math.PI*2;
        public const float Rad2Deg = 180 / PI;
        public const float Deg2Rad = PI / 180;
        public static float Lerp(float a, float b, float t)
            => (b - a) * t + a;
        public static float Clamp(float min, float max, float value)
            => Max(Min(max, value), min);

        public static float Clamp01(float value) => Clamp(0, 1, value);

        public static int RoundToInt(float value) => (int)Math.Round(value);
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);
        public static int FloorToInt(float value) => (int)Math.Floor(value);

        public static int Sign(float value) => Math.Sign(value);

        public static float Max(params Single[] val1) => val1.Max();
        public static float Min(params Single[] val1) => val1.Min();

    }
}
