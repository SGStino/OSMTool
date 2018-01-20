using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.AI.Navigation;
using Simulation.Traffic.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class AgentJobTest
    {
        private RouteData routeMap = new RouteData();
        //public void TestAgent()
        //{
        //    var agent = new Agent();

        //    var start = new[] { routeMap.mR0 };
        //    var end = new[] { routeMap.mR5 };
        //    var routeSolver = new RouteSolver(start, end);

        //    while (routeSolver.Iterate()) { }
        //    Assert.IsTrue(routeSolver.IsSuccess, "RouteSolver.IsSuccess");
        //    var route = routeSolver.Solution;

        //    var first = route.First();
        //    var last = route.Last();

        //    var pathSolver = new PathSolver(first.Paths, last.Paths, route);
        //    while (pathSolver.Iterate()) { }
        //    Assert.IsTrue(pathSolver.IsSuccess, "PathSolver.IsSuccess");
        //    var path = pathSolver.Solution;
        //}

        [TestMethod]
        public void AgentJobTestFindRoute()
        {

            Action<Sequence<IAIPath>> pathCallback = path => { };
            Action<Sequence<IAIRoute>> routeCallback = route => { };
             

            var startRoute = new[] { routeMap.mR0 };
            var endRoute = new[] { routeMap.mR5 };
 
            var start = new[] { routeMap.mR0P0 };
            var end = new[] { routeMap.mR5P1 };


            var job = new AgentJob(pathCallback, routeCallback);

            job.SetSource(startRoute, start);
            job.SetDestination(endRoute, end);

            job.Start();

            while (!job.CurrentState.HasFlag(AgentJobState.Error) && !job.CurrentState.HasFlag(AgentJobState.Completed))
            {
                job.Iterate();
            }

            Assert.IsFalse(job.CurrentState.HasFlag(AgentJobState.Error), "ErrorState: "+job.CurrentState);
            Assert.IsTrue(job.CurrentState.HasFlag(AgentJobState.Completed), "Completed");
        }

        [TestMethod]
        public void AgentJobTestFindNoPath()
        {

            Action<Sequence<IAIPath>> pathCallback = path => { };
            Action<Sequence<IAIRoute>> routeCallback = route => { };


            var startRoute = new[] { routeMap.mR0 };
            var endRoute = new[] { routeMap.mR5 };

            var start = new[] { routeMap.frR4P0 };
            var end = new[] { routeMap.mR5P1 };


            var job = new AgentJob(pathCallback, routeCallback);

            job.SetSource(startRoute, start);
            job.SetDestination(endRoute, end);

            job.Start();

            while (!job.CurrentState.HasFlag(AgentJobState.Error) && !job.CurrentState.HasFlag(AgentJobState.Completed))
            {
                job.Iterate();
            }

            Assert.IsTrue(job.CurrentState.HasFlag(AgentJobState.Error), "ErrorState: " + job.CurrentState);
            Assert.IsFalse(job.CurrentState.HasFlag(AgentJobState.Completed), "Completed");
            Assert.AreEqual(AgentJobState.PathNotFound, job.CurrentState);
        }
        [TestMethod]
        public void AgentJobTestFindNoRoute()
        { 
            Action<Sequence<IAIPath>> pathCallback = path => { };
            Action<Sequence<IAIRoute>> routeCallback = route => { };


            var startRoute = new[] { routeMap.rR5 };
            var endRoute = new[] { routeMap.mR5 };

            var start = new[] { routeMap.frR4P0 };
            var end = new[] { routeMap.mR5P1 };


            var job = new AgentJob(pathCallback, routeCallback);

            job.SetSource(startRoute, start);
            job.SetDestination(endRoute, end);

            job.Start();

            while (!job.CurrentState.HasFlag(AgentJobState.Error) && !job.CurrentState.HasFlag(AgentJobState.Completed))
            {
                job.Iterate();
            }

            Assert.IsTrue(job.CurrentState.HasFlag(AgentJobState.Error), "ErrorState: " + job.CurrentState);
            Assert.IsFalse(job.CurrentState.HasFlag(AgentJobState.Completed), "Completed");
            Assert.AreEqual(AgentJobState.RouteNotFound, job.CurrentState);
        }
        //[TestMethod]
        //public void AgentTestWalkRoute()
        //{
        //    var routeFinder = new DummyRouteFinder(routeMap.mR0);
        //    var agent = new Agent(routeFinder); 

        //    agent.Teleport(routeMap.mR0.StartPosition);
        //}
    }

}
