using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Simulation.Geometry
{
    public static class Voronoi
    {
        public static IList<Cell> GetCells(IList<Triangle> triangles, IList<Vector2> points)
        {
            var trianglesPerVertex = new HashSet<int>[points.Count];

            for (int i = 0; i < trianglesPerVertex.Length; i++)
                trianglesPerVertex[i] = new HashSet<int>();

            for (int i = 0; i < triangles.Count; i++)
            {
                var t = triangles[i];
                trianglesPerVertex[t.p1].Add(i);
                trianglesPerVertex[t.p2].Add(i);
                trianglesPerVertex[t.p3].Add(i);
            }
            var cells = new List<Cell>(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                var vertexTriangles = trianglesPerVertex[i];



                if (vertexTriangles.Count >= 3)
                {
                    var outerPoints = vertexTriangles.SelectMany(t => new[] { triangles[t].p1, triangles[t].p2, triangles[t].p3 }).Where(p => p != i).Distinct().ToArray();

                    var allPoints = new List<int>();
                    allPoints.AddRange(outerPoints);
                    allPoints.AddRange(outerPoints);

                    foreach (var t in vertexTriangles)
                    {
                        var tri = triangles[t];
                        if (tri.p1 != i) allPoints.Remove(tri.p1);
                        if (tri.p2 != i) allPoints.Remove(tri.p2);
                        if (tri.p3 != i) allPoints.Remove(tri.p3);
                    }

                    if (!allPoints.Any())
                        cells.Add(new Cell(vertexTriangles.OrderBy(t => getAngle(t, i, triangles, points)).ToArray(), i));
                }
            }
            return cells;
        }

        private static float getAngle(int t, int i, IList<Triangle> triangles, IList<Vector2> points)
        {
            var triangle = triangles[t];
            var center = (points[triangle.p1] + points[triangle.p2] + points[triangle.p3]) / 3;
            var vertex = points[i];

            var d = vertex - center;
            return MathF.Atan2(d.Y, d.X);
        }
    }

    public struct Cell
    {
        public readonly int Vertex;
        public readonly int[] Points;

        public Cell(int[] points, int vertex) : this()
        {
            Points = points;
            Vertex = vertex;
        }

        public IEnumerable<Edge> Edges => getEdges();

        private IEnumerable<Edge> getEdges()
        {
            yield return new Edge(Points[Points.Length - 1], Points[0]);
            for (int i = 1; i < Points.Length; i++)
                yield return new Edge(Points[i - 1], Points[i]);
        }
    }
}
