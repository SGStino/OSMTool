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
using Simulation.Data;

namespace Simulation.Traffic.Test
{

    [TestClass]
    public class SolverTests
    {
        [TestInitialize]
        public void Start()
        {
            r = new RouteData();
        }

        private RouteData r;

        [TestMethod]
        public void TestRouteSolverToFarLeft()
        {
            var from = r.mR0;
            var to = r.flR5;
            var array = r.sequenceFarLeft;

            TestRoute(from, to, array);
        }

        [TestMethod]
        public void TestRouteSolverToFarRight()
        {
            var from = r.mR0;
            var to = r.frR5;
            var array = r.sequenceFarRight;

            TestRoute(from, to, array);
        }
        [TestMethod]
        public void TestRouteSolverToLeft()
        {
            var from = r.mR0;
            var to = r.lR5;
            var array = r.sequenceLeft;

            TestRoute(from, to, array);
        }

        [TestMethod]
        public void TestRouteSolverToRight()
        {
            var from = r.mR0;
            var to = r.rR5;
            var array = r.sequenceRight;

            TestRoute(from, to, array);
        }
        [TestMethod]
        public void TestRouteSolverToMiddle()
        {
            var from = r.mR0;
            var to = r.mR5;
            var array = r.sequenceMiddle;

            TestRoute(from, to, array);
        }

        private void TestRoute(TestRoute from, TestRoute to, TestRoute[] array)
        {
            var solver = new RouteSolver(new[] { from }, new[] { to });

            while (solver.Iterate())
            { }

            Assert.IsTrue(solver.IsComplete);
            Assert.IsTrue(solver.IsSuccess);

            CollectionAssert.AreEqual(array, solver.Solution.ToArray());
            r.HighlightRoute(array);
        }

        [TestMethod]
        public void TestPathSolverRight()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.rR5P0 };

            var route = r.sequenceRight;
            TestPaths(from, to, route);
        }
        [TestMethod]
        public void TestPathSolverLeft()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.lR5P1 };

            var route = r.sequenceLeft;
            TestPaths(from, to, route);
        }
        [TestMethod]
        public void TestPathSolverFarRight()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.frR5P1 };

            var route = r.sequenceFarRight;
            TestPaths(from, to, route);
        }
        [TestMethod]
        public void TestPathSolverFarLeft()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.flR5P0 };

            var route = r.sequenceFarLeft;
            TestPaths(from, to, route);
        }
        [TestMethod]
        public void TestPathSolverFarMiddle()
        {
            var from = r.mR0.Paths;
            var to = new[] { r.mR5P0 };

            var route = r.sequenceMiddle;
            TestPaths(from, to, route);
        }

        private void TestPaths(IReadOnlyList<IAIPath> from, TestPath[] to, TestRoute[] route)
        {
            var solver = new PathSolver(from, to, route);

            while (solver.Iterate())
            { }

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
        private readonly BehaviorSubjectValue<IEnumerable<IAIPath>> pathsValue;
        private List<TestPath> paths = new List<TestPath>();
        private TestRoute route;

        public TestPath(TestRoute route, TestPath source, int index)
        {
            this.route = route;

            var dir = (route.EndPosition - route.StartPosition).normalized;


            LoftPath = new BehaviorSubjectValue<ILoftPath>(new LinearPath(route.StartPosition, route.EndPosition));
            SideOffsetStart = source == null ? index * 2 : source.SideOffsetEnd;
            SideOffsetEnd = index * 2;
            Index = index;

            this.pathsValue = new BehaviorSubjectValue<IEnumerable<IAIPath>>(paths);
        }
        public int Index { get; }


        public IObservableValue<ILoftPath> LoftPath { get; }

        public float SideOffsetStart { get; }

        public float SideOffsetEnd { get; }

        public float PathOffsetStart => 0;

        public float PathOffsetEnd => 0;

        public IAIPath LeftParralel => route.Paths.OfType<TestPath>().FirstOrDefault(t => t.Index == Index - 1);

        public IAIPath RightParralel => route.Paths.OfType<TestPath>().FirstOrDefault(t => t.Index == Index + 1);

        public bool Reverse => false;

        public float MaxSpeed => 1;

        public float AverageSpeed => 1;

        public IObservableValue<IEnumerable<IAIPath>> NextPaths => pathsValue;

        public LaneType LaneType => LaneType.Road;

        public VehicleTypes VehicleTypes => VehicleTypes.Vehicle;

        public IEnumerable<IAIGraphNode> NextNodes => NextPaths.Value;

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
            StartPosition = new Vector3(level * 10, 0, (int)col * 10);
            EndPosition = new Vector3(level * 10 + 10, 0, (int)col * 10);
            Level = level;
            Col = col;
        }
        private List<TestPath> paths = new List<TestPath>();
        private List<TestRoute> routes = new List<TestRoute>();

        public Vector3 StartPosition { get; private set; }

        public Vector3 EndPosition { get; private set; }

        public float Length => 1;

        public float Speed => 1;

        public float Cost => 1;

        public IReadOnlyList<IAIPath> Paths => paths.ToArray();

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
            route.StartPosition = EndPosition;
            routes.Add(route);
        }

        internal string GetName()
        {
            return $"{Col}{Level}";
        }
    }
}
