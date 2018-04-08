using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using Simulation.Traffic.AI.Agents;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Simulation.Data.Primitives;

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

            Assert.AreEqual(AgentStatus.Initializing, agent.CurrentStatus);
            agent.Teleport(Matrix4x4.CreateTranslation(start));
            Assert.AreEqual(AgentStatus.WaitingForRoute, agent.CurrentStatus);

            Assert.IsNotNull(runner.Job);

            agent.Update(1 / 60.0f);
            Assert.AreEqual(AgentJobStatus.RouteNotFound, runner.Job.CurrentState);



            agent.SetDestination(end);
            Assert.AreEqual(AgentStatus.WaitingForRoute, agent.CurrentStatus);

            Assert.AreEqual(AgentJobStatus.FindingRoute, runner.Job.CurrentState);
            while (runner.Job.CurrentState == AgentJobStatus.FindingRoute)
                runner.Job.Iterate();
            Assert.AreEqual(AgentJobStatus.FindingPath, runner.Job.CurrentState);
            while (runner.Job.CurrentState == AgentJobStatus.FindingPath)
                runner.Job.Iterate();
            Assert.AreEqual(AgentJobStatus.Completed, runner.Job.CurrentState);
            Assert.AreEqual(AgentStatus.WaitingForRoute, agent.CurrentStatus);
            agent.Update(1 / 60.0f);




            var pointHistory = new List<Vector3>();
            Assert.AreEqual(AgentStatus.GoingToRoute, agent.CurrentStatus);
            while (agent.CurrentStatus == AgentStatus.GoingToRoute)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.Zero));
                agent.Update(1 / 60.0f);
            }

            Assert.AreEqual(AgentStatus.FollowingRoute, agent.CurrentStatus);
            while (agent.CurrentStatus == AgentStatus.FollowingRoute)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.Zero));
                agent.Update(1 / 60.0f);
            }

            Assert.AreEqual(AgentStatus.GoingToDestination, agent.CurrentStatus);
            while (agent.CurrentStatus == AgentStatus.GoingToDestination)
            {
                pointHistory.Add(agent.CurrentTransform.MultiplyPoint3x4(Vector3.Zero));
                agent.Update(1 / 60.0f);
            }
            Assert.AreEqual(AgentStatus.DestinationReached, agent.CurrentStatus);

            //var pointsString = string.Join(Environment.NewLine, pointHistory.Select(t => $"{t.X.ToString(CultureInfo.InvariantCulture)}, {t.Z.ToString(CultureInfo.InvariantCulture)}"));

            //var routePoints = string.Join(Environment.NewLine, agent.RouteSequence.SelectMany(t => new[]
            //{
            //    $"{t.StartPosition.X.ToString(CultureInfo.InvariantCulture)}, {t.StartPosition.Z.ToString(CultureInfo.InvariantCulture)}",
            //    $"{t.EndPosition.X.ToString(CultureInfo.InvariantCulture)}, {t.EndPosition.Z.ToString(CultureInfo.InvariantCulture)}"
            //}));
             
        }

        private Vector3 getPoint(TestRoute mR5, float progress, float side)
        {
            var start = mR5.StartPosition;
            var end = mR5.EndPosition;
            var dir = (end - start).Normalized();

            var sideVector = Vector3.Cross(Directions3.Up, dir) * side;

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

        public bool Find(Vector3 point, out IReadOnlyList<IAIRoute> routes, out IReadOnlyList<IAIPath> paths)
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

