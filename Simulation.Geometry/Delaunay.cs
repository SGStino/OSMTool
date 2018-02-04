using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Geometry
{
    public static class Delaunay
    {
        /// <summary>Returns a triangle that encompasses all triangulation Vector2s.</summary>
        /// <param name="triangulationVector2s">A list of triangulation Vector2s.</param>
        /// <returns>Returns a triangle that encompasses all triangulation Vector2s.</returns>
        private static Triangle SuperTriangle(IList<Vector2> triangulationVector2s)
        {
            float M = triangulationVector2s[0].x;

            // get the extremal x and y coordinates
            for (int i = 1; i < triangulationVector2s.Count; i++)
            {
                float xAbs = Math.Abs(triangulationVector2s[i].x);
                float yAbs = Math.Abs(triangulationVector2s[i].y);
                if (xAbs > M) M = xAbs;
                if (yAbs > M) M = yAbs;
            }

            // make a triangle
            var sp1 = new Vector2(10 * M, 0);
            var sp2 = new Vector2(0, 10 * M);
            var sp3 = new Vector2(-10 * M, -10 * M);

            int i1 = triangulationVector2s.Count;
            triangulationVector2s.Add(sp1);
            int i2 = triangulationVector2s.Count;
            triangulationVector2s.Add(sp2);
            int i3 = triangulationVector2s.Count;
            triangulationVector2s.Add(sp3);

            return new Triangle(i1, i2, i3);
        }

        /// <summary>Tests if a point lies in the circumcircle of the triangle.</summary>
        /// <param name="point">point to test </param>
        /// <param name="triangle">triangle to test against</param>
        /// <param name="points">vertex buffer used by the triangle</param>
        /// <returns>For a counterclockwise order of the vertices of the triangle, this test is 
        /// <list type ="bullet">
        /// <item>positive if <paramref name="point"/> lies inside the circumcircle.</item>
        /// <item>zero if <paramref name="point"/> lies on the circumference of the circumcircle.</item>
        /// <item>negative if <paramref name="point"/> lies outside the circumcircle.</item></list></returns>
        /// <remarks>The vertices of the triangle must be arranged in counterclockwise order or the result
        /// of this test will be reversed. This test ignores the z-coordinate of the vertices.</remarks>

        public static float ContainsInCircumcircle(Vector2 point, Triangle triangle, IList<Vector2> points)
        {
            var Vertex1 = points[triangle.p1];
            var Vertex2 = points[triangle.p2];
            var Vertex3 = points[triangle.p3];

            float ax = Vertex1.x - point.x;
            float ay = Vertex1.y - point.y;
            float bx = Vertex2.x - point.x;
            float by = Vertex2.y - point.y;
            float cx = Vertex3.x - point.x;
            float cy = Vertex3.y - point.y;

            float det_ab = ax * by - bx * ay;
            float det_bc = bx * cy - cx * by;
            float det_ca = cx * ay - ax * cy;

            float a_squared = ax * ax + ay * ay;
            float b_squared = bx * bx + by * by;
            float c_squared = cx * cx + cy * cy;

            return a_squared * det_bc + b_squared * det_ca + c_squared * det_ab;
        }

        public static IList<Triangle> Triangulate(IList<Vector2> points)
        {
            var triangulationPoints = new List<Vector2>(points);

            if (triangulationPoints.Count < 3) throw new ArgumentException("Can not triangulate less than three vertices!");

            // The triangle list
            List<Triangle> triangles = new List<Triangle>(); ;

            // The "supertriangle" which encompasses all triangulation points.
            // This triangle initializes the algorithm and will be removed later.
            var superTriangle = SuperTriangle(triangulationPoints);
            triangles.Add(superTriangle);

            // Include each point one at a time into the existing triangulation
            for (int i = 0; i < triangulationPoints.Count; i++)
            {
                // Initialize the edge buffer.
                List<Edge> EdgeBuffer = new List<Edge>();

                // If the actual vertex lies inside the circumcircle, then the three edges of the 
                // triangle are added to the edge buffer and the triangle is removed from list.                             
                for (int j = triangles.Count - 1; j >= 0; j--)
                {
                    Triangle t = triangles[j];
                    if (ContainsInCircumcircle(triangulationPoints[i], t, triangulationPoints) > 0)
                    {
                        EdgeBuffer.Add(new Edge(t.p1, t.p2));
                        EdgeBuffer.Add(new Edge(t.p2, t.p3));
                        EdgeBuffer.Add(new Edge(t.p3, t.p1));
                        triangles.RemoveAt(j);
                    }
                }

                // Remove duplicate edges. This leaves the convex hull of the edges.
                // The edges in this convex hull are oriented counterclockwise!
                for (int j = EdgeBuffer.Count - 2; j >= 0; j--)
                {
                    for (int k = EdgeBuffer.Count - 1; k >= j + 1; k--)
                    {
                        if (EdgeBuffer[j].Equals(EdgeBuffer[k]))
                        {
                            EdgeBuffer.RemoveAt(k);
                            EdgeBuffer.RemoveAt(j);
                            k--;
                            continue;
                        }
                    }
                }

                // Generate new counterclockwise oriented triangles filling the "hole" in
                // the existing triangulation. These triangles all share the actual vertex.
                for (int j = 0; j < EdgeBuffer.Count; j++)
                {
                    triangles.Add(new Triangle(EdgeBuffer[j].p1, EdgeBuffer[j].p2, i));
                }
            }

            // We don't want the supertriangle in the triangulation, so
            // remove all triangles sharing a vertex with the supertriangle.
            for (int i = triangles.Count - 1; i >= 0; i--)
            {
                if (triangles[i].SharesVertexWith(superTriangle))
                    triangles.RemoveAt(i);
            }

            // Return the triangles
            return triangles;
        }

    }
}
