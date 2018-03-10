using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Traffic.AI.Agents
{
    public interface IAgentChain
    {
        IAgentPointer Enter(IAgent agent);
        void Exit(IAgentPointer agent);

        IAgentPointer First { get; }
        IAgentPointer Last { get; }
    }

    public interface IAgentPointer
    {
        IAgentChain Chain { get; }
        IAgentPointer Next { get; }
        IAgent Agent { get; }
        IAgentPointer Previous { get; }
    }
     

    public class LinkedAgentChain : IAgentChain
    {
        public LinkedAgentPointer First { get; private set; }
        public LinkedAgentPointer Last { get; private set; }


        public LinkedAgentPointer Enter(IAgent agent)
        {
            if (First == null && Last == null)
                return Last = First = new LinkedAgentPointer(agent, this);

            var current = Last;

            while (current != null && current.Agent.Progress < agent.Progress)
                current = current.Next;



            LinkedAgentPointer previous, next;


            if (current == null) // we went to far
            {
                previous = First;
                next = null;
            }
            else
            {
                previous = current.Previous;
                next = current;
            }

            current = new LinkedAgentPointer(agent, this);

            current.Next = next;
            if (next != null)
                next.Previous = current;

            current.Previous = previous;
            if (previous != null)
                previous.Next = current;

            if (current.Next == null)
                First = current;
            if (current.Previous == null)
                Last = current;
            return current;

        }

        public void Exit(LinkedAgentPointer pointer)
        {
            var isFirst = pointer == First;
            var isLast = pointer == Last;

            var next = pointer.Next;
            var previous = pointer.Previous;

            if (previous != null)
                previous.Next = next;
            if (next != null)
                next.Previous = previous;

            pointer.Next = pointer.Previous = null; 
            pointer.Chain = null;

            if (isFirst)
                First = previous;
            if (isLast)
                Last = next;
        }

        internal void Clear()
        {
            throw new NotImplementedException();
        }
         

        #region interface access
        void IAgentChain.Exit(IAgentPointer agent) => Exit((LinkedAgentPointer)agent);
        IAgentPointer IAgentChain.Enter(IAgent agent) => Enter(agent);

         

        IAgentPointer IAgentChain.First => First;
        IAgentPointer IAgentChain.Last => Last;
        #endregion
    }

    public static class AgentChainsExtensions
    {
        public static void Disconnect(this IAgentPointer currentPointer)
        {
            currentPointer?.Chain?.Exit(currentPointer);
        }
        public static IAgentPointer Connect(this IAgentChainAIPath path, IAgent agent) => path?.Agents.Enter(agent);

        public static IEnumerable<IAgentPointer> IterateForward(this IAgentChain chain) => IterateForward(chain.Last);

        private static IEnumerable<IAgentPointer> IterateForward(this IAgentPointer current)
        {
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public static IEnumerable<IAgentPointer> IterateBackward(this IAgentChain chain) => IterateBackward(chain.First);

        private static IEnumerable<IAgentPointer> IterateBackward(this IAgentPointer current)
        {
            while (current != null)
            {
                yield return current;
                current = current.Previous;
            }
        }
    }


    public class LinkedAgentPointer : IAgentPointer
    {

        public LinkedAgentPointer(IAgent agent, LinkedAgentChain chain)
        {
            this.Agent = agent;
            this.Chain = chain;
        }

        #region constant
        public LinkedAgentChain Chain { get; internal set; }
        public IAgent Agent { get; }
        #endregion

        #region editable by chain
        public LinkedAgentPointer Next { get; internal set; }
        public LinkedAgentPointer Previous { get; internal set; }
        #endregion

        #region interface access
        IAgentChain IAgentPointer.Chain => Chain;
        IAgentPointer IAgentPointer.Next => Next;
        IAgentPointer IAgentPointer.Previous => Previous;
        #endregion
    }
}
