using System;
using System.Collections.Generic;
using System.Linq;
using Simulation.Traffic.Utilities;
using UnityEngine;

namespace Simulation.Traffic.Lofts
{
    public class ArcLoftPath : ILoftPath
    {
        private readonly float angleDistance;
        private readonly float arcLength;
        private readonly float radius;
        private readonly Vector3 startPosition;
        private readonly Vector3 normal;
        private readonly Vector3 endPosition;
        private readonly Vector3 center;



        public Vector3 Center { get { return center; } }
        public Vector3 StartPoint { get { return startPosition; } }
        public Vector3 Normal { get { return normal; } }
        public float Angle { get { return angleDistance; } }
        public float Radius { get { return radius; } }

        public ArcLoftPath(float startAngle, float angleDistance, float radius, Vector2 center)
            : this(getStartPoint(startAngle, radius), angleDistance, new Vector3(center.x, 0, center.y), new Vector3(0, 1, 0), getStartPoint(startAngle + angleDistance, radius))
        { }

        private static Vector3 getStartPoint(float startAngle, float radius)
        {
            var x = Mathf.Cos(startAngle);
            var z = Mathf.Sin(startAngle);
            return new Vector3(x, 0, z) * radius;
        }
        public ArcLoftPath(Vector3 startPosition, float angleDistance, Vector3 center, Vector3 normal) : this(startPosition, angleDistance, center, normal, GetPosition(angleDistance, normal, startPosition))
        {

        }


        public ArcLoftPath(Vector3 startPosition, float angleDistance, Vector3 center, Vector3 normal, Vector3 endPosition)
        {
            normal.Normalize();

            this.radius = Vector3.Cross(normal, startPosition).magnitude;
            this.arcLength = Mathf.Abs(angleDistance) * radius;
            this.angleDistance = angleDistance;
            this.startPosition = startPosition;
            this.normal = normal;
            this.endPosition = endPosition;
            this.center = center;

            //DebugUtils.DrawCircle(center, radius, Vector3.up, Color.white);

            //var r = Quaternion.AngleAxis(angleDistance * Mathf.Rad2Deg * (Time.time % 1), normal);

            //Debug.DrawRay(center, r * startPosition, Color.green);
            //Debug.Log(radius);
            //if (radius > 100000)
            //{
            //    System.Diagnostics.Debugger.Break();
            //    Debug.DrawLine(startPosition, center, Color.red, 100);
            //    Debug.DrawLine(Quaternion.AngleAxis(angleDistance * Mathf.Rad2Deg, tangent) * startPosition, center, Color.red, 100);
            //}
        }


        public float Length { get { return arcLength; } }

        public Matrix4x4 GetTransform(float progress)
        {
            float angle = progress / arcLength * angleDistance;

            var position = GetPosition(angle, normal, startPosition);

            var side = angleDistance > 0 ? position / radius : -position / radius;

            var forward = Vector3.Cross(normal, side);

            side = Vector3.Cross(Vector3.up, forward).normalized;

            var up = Vector3.Cross(forward, side);

            position += center;

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetColumn(0, side);
            matrix.SetColumn(1, up);
            matrix.SetColumn(2, forward);
            matrix.SetColumn(3, new Vector4(position.x, position.y, position.z, 1));
            return matrix;
        }

        static Vector3 GetPosition(float angle, Vector3 normal, Vector3 startPosition)
        {

            var e = normal.normalized;
            var v = startPosition;
            var cos = Mathf.Cos(angle);
            var sin = Mathf.Sin(angle);

            return cos * v + sin * Vector3.Cross(e, v) + (1 - cos) * Vector3.Dot(e, v) * e;


            //return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, normal) * startPosition;
        }

        public bool GetOffsetToPoint(Vector3 point, out float offset)
        {
            // TODO: optimize
            var plane = new Plane(normal, Center);

            var ray = new Ray(point, Vector3.up);
            float project;
            plane.Raycast(ray, out project);
            //Debug.DrawLine(point, ray.GetPoint(project), Color.green);
            point = ray.GetPoint(project);



            var angle = VectorMath3D.GetAngle(startPosition, point - center, normal);

            offset = angle * radius;

            var planarCenter = this.center + normal * Vector3.Dot(normal, startPosition);

            //Debug.DrawLine(planarCenter, planarCenter + Vector3.up * Mathf.PI, Color.black);
            //Debug.DrawLine(planarCenter, planarCenter + Vector3.up * angle, Color.white);

            if (arcLength > 0)
            {
                if (offset >= 0 && offset <= arcLength)
                    return true;
            }
            else
            {
                if (offset >= arcLength && offset <= 0)
                    return true;
            }
            offset = float.NaN;
            return false;
        }

        public static ILoftPath Create(Vector3 center, Vector3 start, Vector3 end, bool ccw)
        {
            var startVector = start - center;
            var endVector = end - center;
            var normal = Vector3.Cross(startVector, endVector).normalized;
            if (normal.sqrMagnitude < 0.0001f) // 180 deg most likely
                normal = Vector3.up;


            if (Vector3.Dot(normal, Vector3.up) < 0)
                normal = -normal;


            var angle = VectorMath3D.GetAngle(startVector, endVector, normal);
            if (angle < 0)
                angle = Mathf.PI * 2 + angle;

            if (ccw)
                angle = -Mathf.PI * 2 + angle;


            //for (float i = 0; i < Mathf.Abs(angle); i += .1f)
            //{
            //    var n = Quaternion.AngleAxis(Mathf.Rad2Deg * i * Mathf.Sign(angle), normal) * startVector;

            //    Debug.DrawLine(center, center + n, angle > 0 ? Color.green : Color.red);
            //}

            //Debug.DrawLine(center, center + normal, Color.red);
            //Debug.DrawLine(center, start, Color.cyan);
            //Debug.DrawLine(center, end, Color.blue);



            if (Mathf.Abs(angle) < 0.000001f)
                return new LinearPath(start, end);
            else
                return new ArcLoftPath(startVector, angle, center, normal, endVector);
        }

        public void SnapTo(Vector3 to, /*out Vector3 center,*/  out float distance)
        {

            // TODO: not completly accurate



            Vector3 point = VectorMath3D.GetPointOnCircle(normal, radius, to - center);

            float newAngle = VectorMath3D.GetAngle(startPosition, point, normal);



            var angle = angleDistance;


            if (angle < 0)
            {
                newAngle = Mathf.Clamp(newAngle, angle, 0);
            }
            else
            {
                newAngle = Mathf.Clamp(newAngle, 0, angle);
            }


            var clamped = GetPosition(newAngle, normal, startPosition);// Quaternion.AngleAxis(newAngle * Mathf.Rad2Deg, normal) * (startPosition);




            var mag1 = (clamped + center - to).sqrMagnitude;
            var mag2 = (endPosition + center - to).sqrMagnitude;

            if (mag1 > mag2)
            {
                distance = newAngle * radius * Mathf.Sign(angleDistance);//Mathf.Sqrt(mag2);
                //position = endPosition + center;
            }
            else
            {
                //position = clamped + center;
                distance = newAngle * radius * Mathf.Sign(angleDistance);//Mathf.Sqrt(mag1);
            }
        }

        public bool Intersects(Plane plane, out float[] loftDistances)
        {
            if (VectorMath3D.IntersectCircle(plane, center, normal, startPosition, radius, out float a1, out float a2))
            {
                var dists = new List<float>(2);
                var s = Mathf.Sign(Angle);
                a1 *= s;
                a2 *= s;

                if (a1 < 0)
                    a1 += Mathf.PI * 2;

                if (a2 < 0)
                    a2 += Mathf.PI * 2;

                var range = s * Angle;
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


    }
}