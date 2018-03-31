using Simulation.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace Simulation.Leisure.Sports
{
    public class SportsRegistrationCollection : ISportProvider, IDisposable
    {
        private Subject<SportSubscriptionChangeEvent> subject = new Subject<SportSubscriptionChangeEvent>();

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
        public int GetCapacity(SportType type) => data.GetCapacity((int)type);
        public bool Join(SportType type)
        {
            if (data.Add((int)type, 1, prev: out var oldValue, next: out var newValue, capacity: out var capacity))
            {
                subject.OnNext(new SportSubscriptionChangeEvent(type, capacity, capacity, oldValue, newValue));
                return true;
            }
            return false;
        }
        public bool Leave(SportType type)
        {
            if (data.Add((int)type, -1, prev: out var oldValue, next: out var newValue, capacity: out var capacity))
            {
                subject.OnNext(new SportSubscriptionChangeEvent(type, capacity, capacity, oldValue, newValue));
                return true;
            }
            return false;
        }
        public void Clear()
        {
            for (int i = 0; i < VALUES.Length; i++)
            {
                var type = VALUES[i];
                var capacitiy = data.GetCapacity((int)type);
                var value = data.GetValue((int)type);
                if (value != 0)
                    subject.OnNext(new SportSubscriptionChangeEvent(type, capacitiy, capacitiy, 0, value));
            }
            data.Clear();
        }

        public SportsRegistrationCollection SetCapacity(SportType type, int capacity)
        {
            data.SetCapacity((int)type, capacity);
            return this;
        }

        public void Dispose() => subject.OnCompleted();

        public IDisposable Subscribe(IObserver<SportSubscriptionChangeEvent> observer) => subject.Subscribe(observer);
    }

    public struct SportSubscriptionChangeEvent
    {
        public SportSubscriptionChangeEvent(SportType sport, int oldCapacity, int newCapacity, int oldOccupancy, int newOccupancy) : this()
        {
            Sport = sport;
            OldCapacity = oldCapacity;
            NewCapacity = newCapacity;
            OldOccupancy = oldOccupancy;
            NewOccupancy = newOccupancy;
        }

        public SportType Sport { get; }
        public int OldCapacity { get; }
        public int NewCapacity { get; }
        public int OldOccupancy { get; }
        public int NewOccupancy { get; }
    }
}
