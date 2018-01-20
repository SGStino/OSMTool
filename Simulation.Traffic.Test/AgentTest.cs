using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using Simulation.Traffic.AI.Agents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class AgentTest
    {
        private RouteData route = new RouteData();
        [TestMethod]
        public void TestAgent()
        {
            var start = getPoint(route.mR0, 0.5f, -5);
            var end = getPoint(route.mR5, 0.5f, 5);
            var finder = new DummyRouteFinder(start, route.mR0, end, route.flR5);
            var runner = new DummyJobRunner();
            var agent = new Agent(finder, runner);

            Assert.AreEqual(AgentState.Initializing, agent.CurrentState);
            agent.Teleport(Matrix4x4.Translate(start));
            Assert.AreEqual(AgentState.WaitingForRoute, agent.CurrentState);

            Assert.IsNotNull(runner.Job);

            agent.Update(1 / 60.0f);
            Assert.AreEqual(AgentJobState.RouteNotFound, runner.Job.CurrentState);



            agent.SetDestination(end);
            Assert.AreEqual(AgentState.WaitingForRoute, agent.CurrentState);

            Assert.AreEqual(AgentJobState.FindingRoute, runner.Job.CurrentState);
            while (runner.Job.CurrentState == AgentJobState.FindingRoute)
                runner.Job.Iterate();
            Assert.AreEqual(AgentJobState.FindingPath, runner.Job.CurrentState);
            while (runner.Job.CurrentState == AgentJobState.FindingPath)
                runner.Job.Iterate();
            Assert.AreEqual(AgentJobState.Completed, runner.Job.CurrentState);
            Assert.AreEqual(AgentState.WaitingForRoute, agent.CurrentState);
            agent.Update(1 / 60.0f);




            var pointHistory = new List<Vector3>();
            Assert.AreEqual(AgentState.GoingToRoute, agent.CurrentState);
            while (agent.CurrentState == AgentState.GoingToRoute)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.zero));
                agent.Update(1 / 60.0f);
            }

            Assert.AreEqual(AgentState.FollowingRoute, agent.CurrentState);
            while (agent.CurrentState == AgentState.FollowingRoute)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.zero));
                agent.Update(1 / 60.0f);
            }

            Assert.AreEqual(AgentState.GoingToDestination, agent.CurrentState);
            while (agent.CurrentState == AgentState.GoingToDestination)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.zero));
                agent.Update(1 / 60.0f);
            }
            Assert.AreEqual(AgentState.DestinationReached, agent.CurrentState);

            //var pointsString = string.Join(Environment.NewLine, pointHistory.Select(t => $"{t.x.ToString(CultureInfo.InvariantCulture)}, {t.z.ToString(CultureInfo.InvariantCulture)}"));

            //var routePoints = string.Join(Environment.NewLine, agent.RouteSequence.SelectMany(t => new[]
            //{
            //    $"{t.StartPosition.x.ToString(CultureInfo.InvariantCulture)}, {t.StartPosition.z.ToString(CultureInfo.InvariantCulture)}",
            //    $"{t.EndPosition.x.ToString(CultureInfo.InvariantCulture)}, {t.EndPosition.z.ToString(CultureInfo.InvariantCulture)}"
            //}));
             
        }

        private Vector3 getPoint(TestRoute mR5, float progress, float side)
        {
            var start = mR5.StartPosition;
            var end = mR5.EndPosition;
            var dir = (end - start).normalized;

            var sideVector = Vector3.Cross(Vector3.up, dir) * side;

            return Vector3.Lerp(start, end, progress) + sideVector;
        }
    }

    internal class DummyJobRunner : IAgentJobRunner
    {
        public DummyJobRunner()
        {
        }

        public AgentJob Job { get; set; }

        public void Run(AgentJob solverJob)
        {
            Job = solverJob;
        }
    }

    internal class DummyRouteFinder : IAIRouteFinder
    {
        private readonly Vector3 start;
        private TestRoute mR0;
        private readonly Vector3 end;
        private readonly TestRoute endRoute;

        public DummyRouteFinder(Vector3 start, TestRoute startRoute, Vector3 end, TestRoute endRoute)
        {
            this.start = start;
            this.mR0 = startRoute;
            this.end = end;
            this.endRoute = endRoute;
        }

        public bool Find(Vector3 point, out IAIRoute[] routes, out IAIPath[] paths)
        {
            if (point == start)
            {
                routes = new[] { mR0 };
                paths = mR0.Paths;
                return true;
            }
            else if (point == end)
            {
                routes = new[] { endRoute };
                paths = endRoute.Paths;
                return true;
            }

            routes = null;
            paths = null;
            return false;

        }

    }
}

