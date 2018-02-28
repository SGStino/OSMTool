using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI;
using Simulation.Traffic.AI.Agents;
using Simulation.Traffic.Lofts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class AgentStateTest
    {
        [TestMethod]
        public void TestUpdateShort()
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);
            var paths = new List<PathDescription>() { new PathDescription(new DummyAIPath(linearPath), 0, 1) };

            var state = new AgentState(0, 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            state.Update(agent, 1 / 60.0f, getSpeed, paths, out newState);

            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        }

        [TestMethod]
        public void TestUpdateLong()
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);
            var paths = new List<PathDescription>() { new PathDescription(new DummyAIPath(linearPath), 0, 1) };

            var state = new AgentState(0, 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            var result = state.Update(agent, 1 + 1 / 60.0f, getSpeed, paths, out newState);

            Assert.IsTrue(result.HasFlag(AgentStateResult.Moved));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ChangedPath));
            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        }
        [TestMethod]
        public void TestUpdateEnd()
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);
            var paths = new List<PathDescription>() { new PathDescription(new DummyAIPath(linearPath), 0, 1) };

            var state = new AgentState(0, 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            var result = state.Update(agent, 1 + 1 / 60.0f, getSpeed, paths, out newState);

            Assert.IsTrue(result.HasFlag(AgentStateResult.Moved));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ChangedPath));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ReachedEnd));
            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        }
         
    }
    public class DummyStateAgent : IAgent
    {
        AgentState state;

        public DummyStateAgent(AgentState state)
        {
            this.state = state;
        }

        public float Progress => state.Progress;
    }
}
