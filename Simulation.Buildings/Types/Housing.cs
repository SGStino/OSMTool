using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Simulation.People;

namespace Simulation.Buildings.Types
{
    public class Housing : IHousing
    {
        private Subject<HousingEvent> _subject = new Subject<HousingEvent>();
        public Housing(int capacity)
        {
            _inhabitants = new List<IPerson>(capacity);
            this._capacity = capacity;
        }
        private List<IPerson> _inhabitants;
        private readonly int _capacity;

        public IReadOnlyList<IPerson> People => _inhabitants;
        public int Capacity => _inhabitants.Capacity;


        public bool Join(IPerson person)
        {
            if (_inhabitants.Count < _capacity)
            {
                if (!_inhabitants.Contains(person))
                {
                    _inhabitants.Add(person);
                    _subject.OnNext(new HousingEvent(_inhabitants.Count));
                    return true;
                }
            }
            return false;
        }
        public bool Leave(IPerson person)
        {
            if (_inhabitants.Remove(person))
            {
                _subject.OnNext(new HousingEvent(_inhabitants.Count));
                return true;
            }
            return false;
        }

        public IDisposable Subscribe(IObserver<HousingEvent> observer) => _subject.Subscribe(observer);
    }

    public static class HousingExtensions
    {
        public static bool IsVacant(this IHousing housing) => housing.People.Count == 0;
    }
}
