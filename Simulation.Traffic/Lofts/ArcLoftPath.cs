using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Traffic.Utilities;
using System.Numerics;
using Simulation.Data.Primitives;

namespace Simulation.Traffic.Lofts
{
    public class ArcLoftPath : ILoftPath
    {
        public ArcDefinition Definition { get; }

        public ArcLoftPath(ArcDefinition def)
        {
            this.Length = def.Theta * def.R * 2; // theta is half angle

            this.Definition = def;
        }

        public float Length { get; }

        public Matrix4x4 GetTransform(float progress)
        {
            var normal = Definition.Axis;
            var angle = progress / Definition.R;


            //float angle = progress / arcLength * angleDistance;

            Quaternion.CreateFromAxisAngle(Definition.Axis, angle);

            var position = Definition.GetPosition(angle);

            var side = Definition.GetRight(angle); //(position - Definition.Center) * MathF.Sign(angle);



        

            var forward = Vector3.Cross(normal, side);



            side = Vector3.Cross(Directions3.Up, forward).Normalized();

            var up = Vector3.Cross(forward, side);


            return Matrix4x4.CreateWorld(position, forward, up);

            //Matrix4x4 matrix = Matrix4x4.Identity;
            //matrix.SetColumn(0, side);
            //matrix.SetColumn(1, up);
            //matrix.SetColumn(2, forward);
            //matrix.SetColumn(3, new Vector4(position.X, position.Y, position.Z, 1));
            //return matrix;
        }

        static Vector3 GetPosition(float angle, Vector3 normal, Vector3 startPosition)
        {

            var e = normal.Normalized();
            var v = startPosition;
            var cos = MathF.Cos(angle);
            var sin = MathF.Sin(angle);

            return cos * v + sin * Vector3.Cross(e, v) + (1 - cos) * Vector3.Dot(e, v) * e;


            //return Quaternion.AngleAxis(angle * MathF.Rad2Deg, normal) * startPosition;
        }

        public bool GetOffsetToPoint(Vector3 point, out float offset)
        {
            var center = Definition.Center;
            var normal = Definition.Axis;
            var startPosition = Definition.GetStartPosition();
            var radius = Definition.R;
            var fullAngle = Definition.Theta * 2;
            // TODO: optimize
            var plane = new Plane(normal, Vector3.Dot(center, normal));

            var ray = new Ray(point, Directions3.Up);
            float project;
            plane.Raycast(ray, out project);
            //Debug.DrawLine(point, ray.GetPoint(project), Color.green);
            point = ray.GetPoint(project);



            var angle = VectorMath3D.GetAngle(startPosition, point - center, normal);

            offset = angle * radius;

            var planarCenter = center + normal * Vector3.Dot(normal, startPosition);

            //Debug.DrawLine(planarCenter, planarCenter + Directions3.Up * MathF.PI, Color.black);
            //Debug.DrawLine(planarCenter, planarCenter + Directions3.Up * angle, Color.white);

            if (fullAngle > 0)
            {
                if (offset >= 0 && offset <= fullAngle * radius)
                    return true;
            }
            else
            {
                if (offset >= fullAngle * radius && offset <= 0)
                    return true;
            }
            offset = float.NaN;
            return false;
        }

        //public static ILoftPath Create(Vector3 center, Vector3 start, Vector3 end, bool ccw)
        //{
        //    var startVector = start - center;
        //    var endVector = end - center;
        //    var normal = Vector3.Cross(startVector, endVector).Normalized();
        //    if (normal.LengthSquared() < 0.0001f) // 180 deg most likely
        //        normal = Directions3.Up;


        //    if (Vector3.Dot(normal, Directions3.Up) < 0)
        //        normal = -normal;


        //    var angle = VectorMath3D.GetAngle(startVector, endVector, normal);
        //    if (angle < 0)
        //        angle = MathF.PI * 2 + angle;

        //    if (ccw)
        //        angle = -MathF.PI * 2 + angle;


        //    //for (float i = 0; i < MathF.Abs(angle); i += .1f)
        //    //{
        //    //    var n = Quaternion.AngleAxis(MathF.Rad2Deg * i * MathF.Sign(angle), normal) * startVector;

        //    //    Debug.DrawLine(center, center + n, angle > 0 ? Color.green : Color.red);
        //    //}

        //    //Debug.DrawLine(center, center + normal, Color.red);
        //    //Debug.DrawLine(center, start, Color.cyan);
        //    //Debug.DrawLine(center, end, Color.blue);



        //    if (MathF.Abs(angle) < 0.000001f)
        //        return new LinearPath(start, end);
        //    else
        //        return new ArcLoftPath(startVector, angle, center, normal, endVector);
        //}

        public void SnapTo(Vector3 to, /*out Vector3 center,*/  out float distance)
        {

            // TODO: not completly accurate
            var normal = Definition.Axis;
            var radius = Definition.R;
            var center = Definition.Center;
            var angle = Definition.Theta * 2;

            var startPosition = Definition.GetStartPosition();
            var endPosition = Definition.GetEndPosition();

            Vector3 point = VectorMath3D.GetPointOnCircle(normal, radius, to - center);

            float newAngle = VectorMath3D.GetAngle(startPosition, point, normal);





            if (angle < 0)
            {
                newAngle = MathF.Clamp(newAngle, angle, 0);
            }
            else
            {
                newAngle = MathF.Clamp(newAngle, 0, angle);
            }


            var clamped = GetPosition(newAngle, normal, startPosition);// Quaternion.AngleAxis(newAngle * MathF.Rad2Deg, normal) * (startPosition);




            var mag1 = (clamped + center - to).LengthSquared();
            var mag2 = (endPosition + center - to).LengthSquared();

            if (mag1 > mag2)
            {
                distance = newAngle * radius * MathF.Sign(angle);//MathF.Sqrt(mag2);
                //position = endPosition + center;
            }
            else
            {
                //position = clamped + center;
                distance = newAngle * radius * MathF.Sign(angle);//MathF.Sqrt(mag1);
            }
        }

        public bool Intersects(Plane plane, out float[] loftDistances)
        {
            var center = Definition.Center;
            var normal = Definition.Axis;
            var startPosition = Definition.GetStartPosition();
            var radius = Definition.R;
            var angle = Definition.Theta * 2;
            if (VectorMath3D.IntersectCircle(plane, center, normal, startPosition, radius, out float a1, out float a2))
            {
                var dists = new List<float>(2);
                var s = MathF.Sign(angle);
                a1 *= s;
                a2 *= s;

                if (a1 < 0)
                    a1 += MathF.PI * 2;

                if (a2 < 0)
                    a2 += MathF.PI * 2;

                var range = s * angle;
                if (a1 == a2)
                {
                    if (a1 >= 0 && a1 <= range)
                        dists.Add(a1 * radius);
                }
                else
                {
                    if (a1 >= 0 && a1 <= range)
                        dists.Add(a1 * radius);
                    if (a2 >= 0 && a2 <= range)
                        dists.Add(a2 * radius);

                }
                loftDistances = dists.ToArray();
                return dists.Any();
            }
            loftDistances = new float[0];
            return false;
        }

        public Rectangle GetBounds(float width)
        {
            float minX = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float minZ = float.PositiveInfinity;
            float maxZ = float.NegativeInfinity;


            var points = new[] { 0f, 0.5f, 1f };

            var arcLength = Definition.Theta * 2 * Definition.R;

            for (int i = 0; i < 3; i++)
            {
                var m = GetTransform(points[i] * arcLength);
                var offset = m.GetRight() * width;
                var point = m.GetTranslate();

                var min = point - offset;
                var max = point + offset;

                minX = MathF.Min(minX, min.X, max.X);
                minZ = MathF.Min(minZ, min.Z, max.Z);
                maxX = MathF.Max(maxX, max.X, max.X);
                maxZ = MathF.Max(maxZ, max.Z, max.Z);
            }

            return Rectangle.MinMaxRectangle(minX, minZ, maxX, maxZ);
        }
    }
}