using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.Leisure.Sports
{
    public class SportLocationRegistry
    {
        private static readonly int MIN_VALUE;
        private static readonly int MAX_VALUE;
        private static readonly SportType[] VALUES;
        private SportLocationRegistration[] data;
        private Dictionary<ISportProvider, IDisposable> subscriptions = new Dictionary<ISportProvider, IDisposable>();

        static SportLocationRegistry()
        {
            VALUES = Enum.GetValues(typeof(SportType)).Cast<SportType>().ToArray();
            MIN_VALUE = VALUES.Min(t => (int)t);
            MAX_VALUE = VALUES.Max(t => (int)t);
        }

        public SportLocationRegistry()
        {
            this.data = new SportLocationRegistration[MAX_VALUE - MIN_VALUE + 1];
            for (int i = 0; i < data.Length; i++)
                data[i] = new SportLocationRegistration(new HashSet<ISportLocation>());
        }
        public IEnumerable<ISportLocation> this[SportType index] => this.data[(int)index - MIN_VALUE].Locations;

        public void Register(ISportLocation location)
        {
            var provider = location.SportProvider;

            var disp = provider.Subscribe(d => updateRegistrations(d, location));
            subscriptions.Add(provider, disp);
            for (int i = 0; i < data.Length; i++)
            {
                var type = (SportType)(i + MIN_VALUE);

                int vacancies = provider.GetCapacity(type) - provider.GetOccupancy(type);
                if (vacancies > 0)
                    data[i].Locations.Add(location);
            }
        }

        public void Unregister(ISportLocation location)
        {
            var provider = location.SportProvider;
            if (subscriptions.TryGetValue(provider, out var disp))
            {
                subscriptions.Remove(provider);
                disp?.Dispose();
                for (int i = 0; i < data.Length; i++)
                {
                    var type = (SportType)(i + MIN_VALUE);
                    int vacancies = provider.GetCapacity(type) - provider.GetOccupancy(type);
                    if (vacancies > 0)
                        data[i].Locations.Remove(location);
                }
            }
        }

        private void updateRegistrations(SportSubscriptionChangeEvent d, ISportLocation location)
        {
            var newVacencies = d.NewCapacity - d.NewOccupancy;
            var oldVacencies = d.OldCapacity - d.OldOccupancy;
            if (newVacencies > 0 && oldVacencies == 0)
                this.data[((int)d.Sport) - MIN_VALUE].Locations.Add(location);
            else if (newVacencies == 0 && oldVacencies > 0)
                this.data[((int)d.Sport) - MIN_VALUE].Locations.Remove(location);
        }
    }

    internal struct SportLocationRegistration
    {
        public SportLocationRegistration(ISet<ISportLocation> locations)
        {
            Locations = locations;
        }
        public ISet<ISportLocation> Locations { get; }
    }
}
