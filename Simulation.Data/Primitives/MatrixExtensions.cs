using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Simulation.Data.Primitives
{
    public static class MatrixExtensions
    {
        public static Vector4 GetColumn(this Matrix4x4 matrix, int col)
        {
            switch (col)
            {
                case 0:
                    return new Vector4(matrix.M11, matrix.M12, matrix.M13, matrix.M14);
                case 1:
                    return new Vector4(matrix.M21, matrix.M22, matrix.M23, matrix.M24);
                case 2:
                    return new Vector4(matrix.M31, matrix.M32, matrix.M33, matrix.M34);
                case 3:
                    return new Vector4(matrix.M41, matrix.M42, matrix.M43, matrix.M44);
                default:
                    throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
            }
        }

        public static float Get(this Matrix4x4 matrix, int row, int col)
        {
            switch (row)
            {
                case 0:
                    switch (col)
                    {
                        case 0:
                            return matrix.M11;
                        case 1:
                            return matrix.M12;
                        case 2:
                            return matrix.M13;
                        case 3:
                            return matrix.M14;
                        default:
                            throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
                    } 
                case 1:
                    switch (col)
                    {
                        case 0:
                            return matrix.M21;
                        case 1:
                            return matrix.M22;
                        case 2:
                            return matrix.M23;
                        case 3:
                            return matrix.M24;
                        default:
                            throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
                    } 
                case 2:
                    switch (col)
                    {
                        case 0:
                            return matrix.M31;
                        case 1:
                            return matrix.M32;
                        case 2:
                            return matrix.M33;
                        case 3:
                            return matrix.M34;
                        default:
                            throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
                    } 
                case 3:
                    switch (col)
                    {
                        case 0:
                            return matrix.M41;
                        case 1:
                            return matrix.M42;
                        case 2:
                            return matrix.M43;
                        case 3:
                            return matrix.M44;
                        default:
                            throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
                    } 
                default:
                    throw new IndexOutOfRangeException("Matrix4x4 contains only 4 rows");
            }
        }

        public static void SetColumn(ref this Matrix4x4 matrix, int col, Vector3 value)
        {
            switch (col)
            {
                case 0:
                    matrix.M11 = value.X;
                    matrix.M21 = value.Y;
                    matrix.M31 = value.Z;
                    break;
                case 1:
                    matrix.M12 = value.X;
                    matrix.M22 = value.Y;
                    matrix.M32 = value.Z;
                    break;
                case 2:
                    matrix.M13 = value.X;
                    matrix.M23 = value.Y;
                    matrix.M33 = value.Z;
                    break;
                case 3:
                    matrix.M14 = value.X;
                    matrix.M24 = value.Y;
                    matrix.M34 = value.Z;
                    break;
                default:
                    throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
            }
        }
        public static void SetColumn(ref this Matrix4x4 matrix, int col, Vector4 value)
        {
            switch (col)
            {
                case 0:
                    matrix.M11 = value.X;
                    matrix.M21 = value.Y;
                    matrix.M31 = value.Z;
                    matrix.M41 = value.W;
                    break;
                case 1:
                    matrix.M12 = value.X;
                    matrix.M22 = value.Y;
                    matrix.M32 = value.Z;
                    matrix.M42 = value.W;
                    break;
                case 2:
                    matrix.M13 = value.X;
                    matrix.M23 = value.Y;
                    matrix.M33 = value.Z;
                    matrix.M43 = value.W;
                    break;
                case 3:
                    matrix.M14 = value.X;
                    matrix.M24 = value.Y;
                    matrix.M34 = value.Z;
                    matrix.M44 = value.W;
                    break;
                default:
                    throw new IndexOutOfRangeException("Matrix4x4 contains only 4 columns");
            }
        }

        public static Vector3 MultiplyPoint3x4(this Matrix4x4 matrix, Vector3 point)
            => Vector3.Transform(point, matrix);
        public static Vector3 MultiplyVector(this Matrix4x4 matrix, Vector3 vector)
            => Vector3.TransformNormal(vector, matrix);
    }
}
