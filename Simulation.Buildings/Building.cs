using Simulation.Buildings.Types;
using Simulation.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Simulation.Buildings
{
    public class Building
    {
    }

    public class ResidentialBuildingRegistry
    {
        private Dictionary<IResidentialBuilding, IDisposable> buildings = new Dictionary<IResidentialBuilding, IDisposable>();
        private HashSet<IResidentialBuilding> vacantBuildings = new HashSet<IResidentialBuilding>();



        public void Register(IResidentialBuilding building)
        {
            var disp = building.Housings.Select(e => e.VacantHousings > 0).DistinctUntilChanged().Subscribe(e =>
            {
                if (e) vacantBuildings.Add(building);
                else vacantBuildings.Remove(building);
            });
            buildings.Add(building, disp);

            if (building.Housings.Any(h => h.IsVacant()))
                vacantBuildings.Add(building); 
        }
        public void Remove(IResidentialBuilding building)
        {
            if (buildings.TryGetValue(building, out var disp))
            {
                disp?.Dispose();
                buildings.Remove(building);
                vacantBuildings.Remove(building);
            }
        }
        public IReadOnlyCollection<IResidentialBuilding> VacantBuildings => vacantBuildings;
        public IReadOnlyCollection<IResidentialBuilding> Buildings => buildings.Keys;
    }

    public interface IResidentialBuilding
    {
        IHousingCollection Housings { get; }
    }
    public interface IHousingCollection : IReadOnlyList<IHousing>, IObservable<ResidentialBuildingEvent>
    {
        int Inhabitants { get; }
        int Capacity { get; }
    }

    public struct ResidentialBuildingEvent
    {
        public ResidentialBuildingEvent(int vacantHousings) : this()
        {
            VacantHousings = vacantHousings;
        }

        public int VacantHousings { get; }
    }

    public interface IHousing : IObservable<HousingEvent>
    {
        IReadOnlyList<IPerson> People { get; }
        int Capacity { get; }
        bool Join(IPerson person);
        bool Leave(IPerson person);
    }

    public struct HousingEvent
    {
        public HousingEvent(int inhabitants) : this()
        {
            Inhabitants = inhabitants;
        }

        public int Inhabitants { get; }
    }
    public enum HousingVacancyEvent
    {
        BecameOccupied,
        BecameVacant
    }
}
