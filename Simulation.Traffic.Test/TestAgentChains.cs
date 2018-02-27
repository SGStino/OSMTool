using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Traffic.AI.Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.Test
{
    [TestClass]
    public class TestAgentChains
    {
        [TestMethod]
        public void TestEnterEmpty()
        {
            var chain = new LinkedAgentChain();

            var dummyAgent = new DummyAgent() { Progress = 0 };

            var pointer = chain.Enter(dummyAgent);
            Assert.AreSame(dummyAgent, pointer.Agent);

            Assert.AreSame(dummyAgent, chain.First?.Agent);
            Assert.AreSame(dummyAgent, chain.Last?.Agent);

            Assert.IsNull(pointer.Next);
            Assert.IsNull(pointer.Previous);

            Assert.AreSame(chain, pointer.Chain);
        }

        [TestMethod]
        public void TestEnterLast()
        {
            var chain = new LinkedAgentChain();


            var firstAgent = new DummyAgent() { Progress = 1 };
            var firstPointer = chain.Enter(firstAgent);

            var secondAgent = new DummyAgent() { Progress = 0 };
            var secondPointer = chain.Enter(secondAgent);
            Assert.AreSame(secondAgent, secondPointer.Agent);


            Assert.AreSame(firstAgent, chain.First?.Agent);
            Assert.AreSame(secondAgent, chain.Last?.Agent);

            Assert.AreSame(secondPointer, firstPointer.Previous);
            Assert.AreSame(firstPointer, secondPointer.Next);

            Assert.IsNull(firstPointer.Next);
            Assert.IsNull(secondPointer.Previous);

            Assert.AreSame(chain, secondPointer.Chain);
        }
        [TestMethod]
        public void TestEnterFirst()
        {
            var chain = new LinkedAgentChain();


            var firstAgent = new DummyAgent() { Progress = 1 };
            var firstPointer = chain.Enter(firstAgent);

            var secondAgent = new DummyAgent() { Progress = 2 };
            var secondPointer = chain.Enter(secondAgent);
            Assert.AreSame(firstAgent, chain.Last?.Agent);


            Assert.AreSame(firstAgent, chain.Last?.Agent);
            Assert.AreSame(secondAgent, chain.First?.Agent);

            Assert.AreSame(secondPointer, firstPointer.Next);
            Assert.AreSame(firstPointer, secondPointer.Previous);

            Assert.IsNull(firstPointer.Previous);
            Assert.IsNull(secondPointer.Next);

            Assert.AreSame(chain, secondPointer.Chain);
        }

        [TestMethod]
        public void TestEnterMiddle()
        {
            var chain = new LinkedAgentChain();

            var firstAgent = new DummyAgent() { Progress = 2 };
            var firstPointer = chain.Enter(firstAgent);

            var lastAgent = new DummyAgent() { Progress = 0 };
            var lastPointer = chain.Enter(lastAgent);

            var middleAgent = new DummyAgent() { Progress = 1 };
            var middlePointer = chain.Enter(middleAgent);
            Assert.AreSame(middleAgent, middlePointer.Agent);


            Assert.AreSame(firstAgent, chain.First?.Agent);
            Assert.AreSame(lastAgent, chain.Last?.Agent);

            Assert.AreSame(middlePointer, firstPointer.Previous);
            Assert.AreSame(middlePointer, lastPointer.Next);


            Assert.AreSame(firstPointer, middlePointer.Next);
            Assert.AreSame(lastPointer, middlePointer.Previous);

            Assert.AreSame(chain, middlePointer.Chain);
        }

        [TestMethod]
        public void TestExitMiddle()
        {
            var chain = new LinkedAgentChain();

            var firstAgent = new DummyAgent() { Progress = 2 };
            var firstPointer = chain.Enter(firstAgent);

            var lastAgent = new DummyAgent() { Progress = 0 };
            var lastPointer = chain.Enter(lastAgent);

            var middleAgent = new DummyAgent() { Progress = 1 };
            var middlePointer = chain.Enter(middleAgent);

            chain.Exit(middlePointer);

            Assert.AreSame(firstPointer, chain.First);
            Assert.AreSame(lastPointer, chain.Last);

            Assert.AreSame(firstPointer, lastPointer.Next);
            Assert.AreSame(lastPointer, firstPointer.Previous);

            Assert.IsNull(lastPointer.Previous);
            Assert.IsNull(firstPointer.Next);

            Assert.IsNull(middlePointer.Chain);
        }

        [TestMethod]
        public void TestExitFirst()
        {
            var chain = new LinkedAgentChain();

            var firstAgent = new DummyAgent() { Progress = 2 };
            var firstPointer = chain.Enter(firstAgent);

            var lastAgent = new DummyAgent() { Progress = 0 };
            var lastPointer = chain.Enter(lastAgent);


            chain.Exit(firstPointer);

            Assert.AreSame(lastPointer, chain.First);
            Assert.AreSame(lastPointer, chain.Last);


            Assert.IsNull(lastPointer.Previous);
            Assert.IsNull(lastPointer.Next);
            Assert.IsNull(firstPointer.Previous);
            Assert.IsNull(firstPointer.Next);

            Assert.IsNull(firstPointer.Chain);
        }
        [TestMethod]
        public void TestExitLast()
        {
            var chain = new LinkedAgentChain();

            var firstAgent = new DummyAgent() { Progress = 2 };
            var firstPointer = chain.Enter(firstAgent);

            var lastAgent = new DummyAgent() { Progress = 0 };
            var lastPointer = chain.Enter(lastAgent);


            chain.Exit(lastPointer);

            Assert.AreSame(firstPointer, chain.First);
            Assert.AreSame(firstPointer, chain.Last);


            Assert.IsNull(lastPointer.Previous);
            Assert.IsNull(lastPointer.Next);
            Assert.IsNull(firstPointer.Previous);
            Assert.IsNull(firstPointer.Next);

            Assert.IsNull(lastPointer.Chain);
        }

        [TestMethod]
        public void TestExitSingle()
        {
            var chain = new LinkedAgentChain();
            var dummyAgent = new DummyAgent() { Progress = 0 };
            var pointer = chain.Enter(dummyAgent);

            chain.Exit(pointer);

            Assert.IsNull(pointer.Chain);
            Assert.IsNull(pointer.Next);
            Assert.IsNull(pointer.Previous);
            Assert.IsNull(chain.First);
            Assert.IsNull(chain.Last);
        }


        [TestMethod]
        public void TestFull()
        {
            var chain = new LinkedAgentChain();

            var agents = new List<DummyAgent>();
            var pointers = new List<LinkedAgentPointer>();
            for (int i = 0; i < 10; i++)
            {
                foreach (var agent in agents)
                    agent.Progress++;

                var newAgent = new DummyAgent() { Progress = 0 };
                agents.Add(newAgent);
                pointers.Add(chain.Enter(newAgent));

                if (i == 0)
                {
                    Assert.AreEqual(pointers[i], chain.First);
                    Assert.AreEqual(pointers[i], chain.Last);
                    Assert.IsNull(pointers[i].Next);
                    Assert.IsNull(pointers[i].Previous);
                }
                else
                {
                    Assert.AreEqual(pointers[i], chain.Last);
                    Assert.AreEqual(pointers[0], chain.First);

                    Assert.AreEqual(pointers[i - 1], pointers[i].Next);
                    Assert.AreEqual(pointers[i], pointers[i - 1].Previous);
                    Assert.IsNull(pointers[i].Previous);
                    for (int j = 1; j < i; j++)
                    {
                        Assert.AreEqual(pointers[j - 1], pointers[j].Next);
                        Assert.AreEqual(pointers[j], pointers[j - 1].Previous);
                    }
                }
            }
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(pointers[i], chain.First);
                chain.Exit(pointers[i]);

                Assert.IsNull(pointers[i].Chain);
                Assert.IsNull(pointers[i].Next);
                Assert.IsNull(pointers[i].Previous);

                if (i == 9)
                {
                    Assert.IsNull(chain.First);
                    Assert.IsNull(chain.Last);
                }
                else if (i == 8)
                {
                    Assert.AreEqual(chain.First, chain.Last);
                }
            }
        }
    }

    internal class DummyAgent : IAgent
    {
        public DummyAgent()
        {
        }

        public float Progress { get; set; }
    }
}
