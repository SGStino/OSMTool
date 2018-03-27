using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Data
{
    public struct RegistrationCollection
    {
        private readonly int minIndex;
        private readonly int maxIndex;

        private readonly int[] data;
        private readonly int[] capacity;

        public RegistrationCollection(int min, int max)
        {
            minIndex = Math.Min(min, max);
            maxIndex = Math.Max(min, max);
            data = new int[maxIndex - minIndex];
            capacity = new int[data.Length];
        }
        public void SetCapacity(int index, int value)
        {
            capacity[index - minIndex] = value;
        }
        public void SetValue(int index, int value)
        {
            data[index - minIndex] = value;
        }
        public int GetValue(int index) => data[index - minIndex];
        public int GetCapacity(int index) => capacity[index - minIndex];

        public bool Add(int index, int value)
        {
            var prev = GetValue(index);
            var capacity = GetCapacity(index);
            var next = prev + value;
            if (next < 0)
            {
                SetValue(index, 0);
                return false;
            }
            if (next > capacity)
            {
                SetValue(index, capacity);
                return false;
            }
            data[index - minIndex] = next;
            return true;
        }

        public void Clear() => Array.Clear(data, 0, data.Length);
    }

    public struct Registration
    {
        public Registration(int count, int capacity)
        {
            Count = count;
            Capacity = capacity;
        }

        public int Count { get; }
        public int Capacity { get; }
    }
}
