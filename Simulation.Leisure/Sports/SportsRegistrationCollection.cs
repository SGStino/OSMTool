using Simulation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Leisure.Sports
{
    public class SportsRegistrationCollection : ISportProvider
    {
        private static readonly int MIN_VALUE;
        private static readonly int MAX_VALUE;
        private static readonly SportType[] VALUES;
        static SportsRegistrationCollection()
        {
            VALUES = Enum.GetValues(typeof(SportType)).Cast<SportType>().ToArray();
            MIN_VALUE = VALUES.Min(t => (int)t);
            MAX_VALUE = VALUES.Max(t => (int)t);
        }

        public SportsRegistrationCollection()
        {
            data = new RegistrationCollection(MIN_VALUE, MAX_VALUE);
        }
        private RegistrationCollection data;

        public IEnumerable<SportType> AvailableSportTypes => VALUES.Where(t => data.GetCapacity((int)t) > 0);

        public int GetOccupancy(SportType type) => data.GetValue((int)type);
        public int GetCapacity(SportType type) => data.GetValue((int)type);
        public bool Join(SportType type) => data.Add((int)type, 1);
        public bool Leave(SportType type) => data.Add((int)type, -1);
        public void Clear() => data.Clear();

        public SportsRegistrationCollection SetCapacity(SportType type, int capacity)
        {
            data.SetCapacity((int)type, capacity);
            return this;
        }
    }
}
