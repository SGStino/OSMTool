using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static partial class MathF
    {
        public static float Acos(Single d) => (float)Math.Acos(d);                
        public static float Asin(Single d) => (float)Math.Asin(d);                
        public static float Atan(Single d) => (float)Math.Atan(d);                
        public static float Atan2(Single y, Single x) => (float)Math.Atan2(y, x);                
        public static float Ceiling(Single a) => (float)Math.Ceiling(a);                
        public static float Cos(Single d) => (float)Math.Cos(d);                
        public static float Cosh(Single value) => (float)Math.Cosh(value);                
        public static float Floor(Single d) => (float)Math.Floor(d);                
        public static float Sin(Single a) => (float)Math.Sin(a);                
        public static float Tan(Single a) => (float)Math.Tan(a);                
        public static float Sinh(Single value) => (float)Math.Sinh(value);                
        public static float Tanh(Single value) => (float)Math.Tanh(value);                
        public static float Round(Single a) => (float)Math.Round(a);                
        public static float Round(Single value, Int32 digits) => (float)Math.Round(value, digits);                
        public static float Round(Single value, MidpointRounding mode) => (float)Math.Round(value, mode);                
        public static float Round(Single value, Int32 digits, MidpointRounding mode) => (float)Math.Round(value, digits, mode);                
        public static float Truncate(Single d) => (float)Math.Truncate(d);                
        public static float Sqrt(Single d) => (float)Math.Sqrt(d);                
        public static float Log(Single d) => (float)Math.Log(d);                
        public static float Log(Single a, Single newBase) => (float)Math.Log(a, newBase);                
        public static float Log10(Single d) => (float)Math.Log10(d);                
        public static float Exp(Single d) => (float)Math.Exp(d);                
        public static float Pow(Single x, Single y) => (float)Math.Pow(x, y);                
        public static float IEEERemainder(Single x, Single y) => (float)Math.IEEERemainder(x, y);                
        public static float Abs(Single value) => (float)Math.Abs(value);                
        public static float Max(Single val1, Single val2) => (float)Math.Max(val1, val2);                
        public static float Min(Single val1, Single val2) => (float)Math.Min(val1, val2);                
    }
}
