using Simulation.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Simulation.Buildings.Types
{
    public class Appartment : Building, IResidentialBuilding
    {
        public Appartment(params int[] appartmentSizes)
        {
            Housings = HousingBuilder.Create(appartmentSizes).Build();
        }

        public IHousingCollection Housings { get; }
    }


    public class HousingCollection : IHousingCollection
    {
        private Housing[] housings;
        private IObservable<ResidentialBuildingEvent> housingEvents;

        public HousingCollection(Housing[] housings)
        {
            this.housings = housings;

            this.housingEvents = housings.Select((h, i) => h.Select(e => e.Inhabitants == 0).DistinctUntilChanged()).Merge().Select(t => new ResidentialBuildingEvent(housings.Count(HousingExtensions.IsVacant))).Publish().RefCount();
        }



        public IHousing this[int index] => housings[index];

        public int Inhabitants => housings.Sum(s => s.People.Count);
        public int Capacity => housings.Sum(s => s.Capacity);

        public int Count => housings.Length;

        public IEnumerator<IHousing> GetEnumerator() => ((IEnumerable<IHousing>)housings).GetEnumerator();

        public IDisposable Subscribe(IObserver<ResidentialBuildingEvent> observer) => housingEvents.Subscribe(observer);

        IEnumerator IEnumerable.GetEnumerator() => housings.GetEnumerator();


    }

    public class HousingBuilder
    {
        public static HousingBuilder Create(params int[] capacity) => new HousingBuilder().Add(capacity);

        private List<Housing> housings = new List<Housing>();
        public HousingBuilder Add(params int[] capacity)
        {
            housings.AddRange(capacity.Select(t => new Housing(t)));
            return this;
        }

        public HousingCollection Build() => new HousingCollection(housings.ToArray());
    }
}
