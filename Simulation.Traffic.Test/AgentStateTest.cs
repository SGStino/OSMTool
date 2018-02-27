using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            var state = new AgentState(new PathDescription(new DummyAIPath(linearPath), 0, 1), 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            state.Update(agent, 1 / 60.0f, getSpeed, nextPathNotAllowed, out newState);

            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        }

        [TestMethod]
        public void TestUpdateLong()
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);

            var state = new AgentState(new PathDescription(new DummyAIPath(linearPath), 0, 1), 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            var result = state.Update(agent, 1 + 1 / 60.0f, getSpeed, NextPathTrue, out newState);

            Assert.IsTrue(result.HasFlag(AgentStateResult.Moved));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ChangedPath));
            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        } 
        [TestMethod]
        public void TestUpdateEnd()
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);

            var state = new AgentState(new PathDescription(new DummyAIPath(linearPath), 0, 1), 0, null);

            var agent = new DummyStateAgent(state);

            GetSpeedDelegate getSpeed = s => 1.0f;

            AgentState newState;
            var result = state.Update(agent, 1 + 1 / 60.0f, getSpeed, nextPathFalse, out newState);

            Assert.IsTrue(result.HasFlag(AgentStateResult.Moved));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ChangedPath));
            Assert.IsTrue(result.HasFlag(AgentStateResult.ReachedEnd));
            Assert.AreEqual(1 / 60.0f, newState.Progress, 0.000001f);
        }

        private bool nextPathFalse(AgentState state, out PathDescription path)
        {
            path = default(PathDescription);
            return false;
        }

        private bool NextPathTrue(AgentState state, out PathDescription path)
        {
            var linearPath = new LinearPath(Vector3.zero, Vector3.forward);

            path = new PathDescription(new DummyAIPath(linearPath), 0, 1);
            return true;
        }

        private bool nextPathNotAllowed(AgentState state, out PathDescription path)
        {
            throw new NotImplementedException();
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
