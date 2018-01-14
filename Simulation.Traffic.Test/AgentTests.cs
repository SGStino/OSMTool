using Simulation.Traffic.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Simulation.Traffic.Lofts;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI.Navigation;
using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Utilities;

namespace Simulation.Traffic.Test
{
    public enum TestRouteColumn
    {
        FarLeft = -2,
        Left = -1,
        Middle = 0,
        Right = 1,
        FarRight = 2
    }
    internal class RouteData
    {
        public readonly TestRoute[] inputRoutes;
        public readonly TestPath[] inputPaths;
        public readonly TestRoute[] outputRoutes;
        public readonly TestPath[] outputPaths;
        public readonly TestRoute[] sequenceMiddle;
        public readonly TestRoute[] sequenceLeft;
        public readonly TestRoute[] sequenceFarLeft;
        public readonly TestRoute[] sequenceRight;
        public readonly TestRoute[] sequenceFarRight;
        public readonly TestRoute mR0;
        public readonly TestPath mR0P0;
        public readonly TestPath mR0P1;
        public readonly TestPath mR0P2;
        public readonly TestRoute lR1;
        public readonly TestRoute mR1;
        public readonly TestPath lR1P0;
        public readonly TestPath mR1P0;
        public readonly TestPath mR1P1;
        public readonly TestRoute lR2;
        public readonly TestPath lR2P0;
        public readonly TestRoute mR2;
        public readonly TestPath mR2P0;
        public readonly TestRoute rR2;
        public readonly TestPath rR2P0;
        public readonly TestRoute lR3;
        public readonly TestPath lR3P0;
        public readonly TestPath lR3P1;
        public readonly TestRoute mR3;
        public readonly TestPath mR3P0;
        public readonly TestPath mR3P1;
        public readonly TestRoute rR3;
        public readonly TestPath rR3P0;
        public readonly TestPath rR3P1;
        public readonly TestRoute flR4;
        public readonly TestPath flR4P0;
        public readonly TestRoute lR4;
        public readonly TestPath lR4P0;
        public readonly TestRoute mR4;
        public readonly TestPath mR4P0;
        public readonly TestPath mR4P1;
        public readonly TestRoute frR4;
        public readonly TestPath frR4P0;
        public readonly TestPath frR4P1;
        public readonly TestRoute flR5;
        public readonly TestPath flR5P0;
        public readonly TestRoute lR5;
        public readonly TestPath lR5P0;
        public readonly TestPath lR5P1;
        public readonly TestRoute mR5;
        public readonly TestPath mR5P0;
        public readonly TestPath mR5P1;
        public readonly TestRoute rR5;
        public readonly TestPath rR5P0;
        public readonly TestRoute frR5;
        public readonly TestPath frR5P0;
        public readonly TestPath frR5P1;
        private GraphViz routesGraph;
        private GraphViz pathsGraph;

        public RouteData()
        {
            var fl = TestRouteColumn.FarLeft;
            var l = TestRouteColumn.Left;
            var m = TestRouteColumn.Middle;
            var r = TestRouteColumn.Right;
            var fr = TestRouteColumn.FarRight;
            // 0
            mR0 = TR(0, m);

            mR0P0 = TP(mR0);
            mR0P1 = TP(mR0);
            mR0P2 = TP(mR0);

            // 1
            lR1 = TR(1, l, mR0);
            mR1 = TR(1, m, mR0);

            lR1P0 = TP(lR1, mR0P0);
            mR1P0 = TP(mR1, mR0P1);
            mR1P1 = TP(mR1, mR0P2);

            // 2
            lR2 = TR(2, l, lR1);

            lR2P0 = TP(lR2, lR1P0);

            mR2 = TR(2, m, mR1);
            mR2P0 = TP(mR2, mR1P0);


            rR2 = TR(2, r, mR1);
            rR2P0 = TP(rR2, mR1P1);

            // 3
            lR3 = TR(3, l, lR2);

            lR3P0 = TP(lR3, lR2P0);
            lR3P1 = TP(lR3, lR2P0);

            mR3 = TR(3, m, mR2);
            mR3P0 = TP(mR3, mR2P0);
            mR3P1 = TP(mR3, mR2P0);

            rR3 = TR(3, r, rR2);
            rR3P0 = TP(rR3, rR2P0);
            rR3P1 = TP(rR3, rR2P0);

            // 4

            flR4 = TR(4, fl, lR3);

            flR4P0 = TP(flR4, lR3P0);

            lR4 = TR(4, l, lR3);

            lR4P0 = TP(lR4, lR3P1);

            mR4 = TR(4, m, mR3);
            mR4P0 = TP(mR4, mR3P0);
            mR4P1 = TP(mR4, mR3P1);

            frR4 = TR(4, fr, rR3);

            frR4P0 = TP(frR4, rR3P0);
            frR4P1 = TP(frR4, rR3P1);

            // 5

            flR5 = TR(5, fl, flR4);

            flR5P0 = TP(flR5, flR4P0);

            lR5 = TR(5, l, lR4);

            lR5P0 = TP(lR5, lR4P0);
            lR5P1 = TP(lR5, lR4P0);

            mR5 = TR(5, m, mR4);

            mR5P0 = TP(mR5, mR4P0);
            mR5P1 = TP(mR5, mR4P1);

            rR5 = TR(5, r, mR4);
            rR5P0 = TP(rR5, mR4P1);

            frR5 = TR(5, fr, frR4);
            frR5P0 = TP(frR5, frR4P0);
            frR5P1 = TP(frR5, frR4P1);



            inputRoutes = new[] { mR0 };
            inputPaths = new[] { mR0P0, mR0P1, mR0P2 };

            outputRoutes = new[] { flR5, lR5, mR5, rR5, frR5 };
            outputPaths = new[] { flR5P0, lR5P0, lR5P1, mR5P0, mR5P1, rR5P0, frR5P0, frR5P1 };

            sequenceRight = new[] { mR0, mR1, mR2, mR3, mR4, rR5 };
            sequenceMiddle = new[] { mR0, mR1, mR2, mR3, mR4, mR5 };
            sequenceLeft = new[] { mR0, lR1, lR2, lR3, lR4, lR5 };
            sequenceFarLeft = new[] { mR0, lR1, lR2, lR3, flR4, flR5 };
            sequenceFarRight = new[] { mR0, mR1, rR2, rR3, frR4, frR5 };

            spitOutGraph();
        }

        private void spitOutGraph()
        {
            routesGraph = new GraphViz();
            pathsGraph = new GraphViz();
            addToGraph(mR0, routesGraph, pathsGraph);

            var str = routesGraph.ToString();
            var str2 = pathsGraph.ToString();
        }

        private void addToGraph(TestRoute mR0, GraphViz graph, GraphViz pathsGraph)
        {
            graph.Add(mR0.GetName());
            graph.SetGroup("level" + mR0.Level, mR0.GetName());
            foreach (TestRoute next in mR0.NextRoutes)
            {
                graph.Connect(mR0.GetName(), next.GetName());
                addToGraph(next, graph, pathsGraph);
            }

            foreach (TestPath path in mR0.Paths)
            {
                pathsGraph.SetGroup(mR0.GetName(), path.GetName());
                var left = path.LeftParralel as TestPath;
                if (left != null)
                    pathsGraph.Connect(path.GetName(), left.GetName());
                var right = path.RightParralel as TestPath;
                if (right != null)
                    pathsGraph.Connect(path.GetName(), right.GetName());
                foreach (TestPath next in path.NextPaths)
                    pathsGraph.Connect(path.GetName(), next.GetName());
            }
        }

        private TestRoute TR(int level, TestRouteColumn col, TestRoute source = null)
        {
            var route = new TestRoute(level, col);
            if (source != null)
                source.Connect(route);
            return route;
        }
        private TestPath TP(TestRoute route, TestPath source = null)
        {
            var p = new TestPath(route, source, route.Paths.Count());
            route.Add(p);
            if (source != null)
                source.Connect(p);
            return p;
        }

        internal void HighlightRoute(IEnumerable<TestRoute> array)
        {
            var colorDict = new Dictionary<string, string> { { "color", "red" } };
            TestRoute prev = null;
            foreach (TestRoute item in array)
            {
                routesGraph.Add(item.GetName(), colorDict);
                pathsGraph.GroupDetails(item.GetName(), colorDict);
                if (prev != null)
                    routesGraph.Connect(prev.GetName(), item.GetName(), colorDict);
                prev = item;
            }
        }

        internal void HighlightRoute(IEnumerable<TestPath> solution)
        {
            var colorDict = new Dictionary<string, string> { { "color", "green" } };

            foreach(var path in solution)
            {
                pathsGraph.Add(path.GetName(), colorDict); 
            }
        }
    }

    [TestClass]
    public class AgentTests
    {

        private RouteData r = new RouteData();



        [TestMethod]
        public void TestRouteSolverToFarLeft()
        {
            var from = r.mR0;
            var to = r.flR5;

            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            var array = r.sequenceFarLeft;

            r.HighlightRoute(array);
            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }

        [TestMethod]
        public void TestRouteSolverToFarRight()
        {
            var from = r.mR0;
            var to = r.frR5;

            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            var array = r.sequenceFarRight;

            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }
        [TestMethod]
        public void TestRouteSolverToLeft()
        {
            var from = r.mR0;
            var to = r.lR5;

            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            var array = r.sequenceLeft;

            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }

        [TestMethod]
        public void TestRouteSolverToRight()
        {
            var from = r.mR0;
            var to = r.rR5;

            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            var array = r.sequenceRight;
            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }
        [TestMethod]
        public void TestRouteSolverToMiddle()
        {
            var from = r.mR0;
            var to = r.mR5;

            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            var array = r.sequenceMiddle;
            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }

        [TestMethod]
        public void TestPathSolver()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.rR5P0 };

            var route = r.sequenceRight;
            var solver = new PathSolver(from, to, route);

            while (solver.Iterate())
            { }

            var graph = solver.GetGraph(t => (t as TestPath).GetName());
            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);
            r.HighlightRoute(route);
            r.HighlightRoute(solver.Solution.OfType<TestPath>());
        }

        private string getUniqueId(IAIRoute arg)
        {
            return (arg as TestRoute)?.GetName();
        }
    }

    internal class TestPath : IAIPath
    {
        private List<TestPath> paths = new List<TestPath>();
        private TestRoute route;

        public TestPath(TestRoute route, TestPath source, int v)
        {
            this.route = route;

            LoftPath = new LinearPath(route.StartPosition, route.EndPosition);
            SideOffsetStart = source == null ? v * 2 : source.SideOffsetEnd;
            SideOffsetEnd = v * 2;
            Index = v;
        }
        public int Index { get; }
        public ILoftPath LoftPath { get; }

        public float SideOffsetStart { get; }

        public float SideOffsetEnd { get; }

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public IAIPath LeftParralel => route.Paths.OfType<TestPath>().FirstOrDefault(t => t.Index == Index - 1);

        public IAIPath RightParralel => route.Paths.OfType<TestPath>().FirstOrDefault(t => t.Index == Index + 1);

        public bool Reverse => false;

        public float MaxSpeed => 1;

        public float AverageSpeed => 1;

        public IEnumerable<IAIPath> NextPaths => paths;

        public LaneType LaneType => LaneType.Road;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths;

        internal void Connect(TestPath p)
        {
            paths.Add(p);
        }

        internal string GetName()
        {
            return route.GetName() + "." + Index;
        }
    }

    internal class TestRoute : IAIRoute
    {
        public TestRoute(int level, TestRouteColumn col)
        {
            StartPosition = new Vector3(level * 10, (int)col * 10);
            EndPosition = new Vector3(level * 10 + 10, (int)col * 10);
            Level = level;
            Col = col;
        }
        private List<TestPath> paths = new List<TestPath>();
        private List<TestRoute> routes = new List<TestRoute>();

        public Vector3 StartPosition { get; }

        public Vector3 EndPosition { get; }

        public float Length => 1;

        public float Speed => 1;

        public float Cost => 1;

        public IAIPath[] Paths => paths.ToArray();

        public IEnumerable<IAIRoute> NextRoutes => routes;

        public IEnumerable<IAIGraphNode> NextNodes => routes;

        public int Level { get; }
        public TestRouteColumn Col { get; }

        internal void Add(TestPath p)
        {
            paths.Add(p);
        }

        internal void Connect(TestRoute route)
        {
            routes.Add(route);
        }

        internal string GetName()
        {
            return $"{Col}{Level}";
        }
    }
}
