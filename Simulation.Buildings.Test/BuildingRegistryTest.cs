using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Buildings.Types;
using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;
using Simulation.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
namespace Simulation.Buildings.Test
{
    [TestClass]
    public class BuildingRegistryTest
    {
        [TestMethod]
        public void TestHousingEvents()
        {
            var housing = new Housing(3);

            int count = 0;
            Action<HousingEvent> handler = t =>
            {
                Assert.AreEqual(count, t.Inhabitants);
            };

            var a = new DummyPerson();
            var b = new DummyPerson();
            var c = new DummyPerson();
            var d = new DummyPerson();

            housing.Select(t => t).Subscribe(handler);
            count++;
            Assert.IsTrue(housing.Join(a));
            count++;
            Assert.IsTrue(housing.Join(b));
            Assert.IsFalse(housing.Join(b));
            count++;
            Assert.IsTrue(housing.Join(c));
            Assert.IsFalse(housing.Join(d));

            count--;
            Assert.IsTrue(housing.Leave(a));
            count--;
            Assert.IsTrue(housing.Leave(b));
            Assert.IsFalse(housing.Leave(b));
            count--;
            Assert.IsTrue(housing.Leave(c));
            Assert.IsFalse(housing.Leave(d));
        }

        [TestMethod]
        public void TestCollection()
        {
            var housingCollection = HousingBuilder.Create(10, 10).Build();

            int vacantCount = 2;
            housingCollection.Subscribe(n =>
            {
                Assert.AreEqual(vacantCount, n.VacantHousings);
            });

            var a = new DummyPerson();
            var b = new DummyPerson();
            var c = new DummyPerson();

            vacantCount--;
            housingCollection[0].Join(a);
            //vacantCount++;
            housingCollection[0].Join(b);
            vacantCount--;
            housingCollection[1].Join(c);

            housingCollection[0].Leave(a);
            vacantCount++;
            housingCollection[0].Leave(b);
        }

        [TestMethod]
        public void TestResidentialRegistry()
        {
            var registry = new ResidentialBuildingRegistry();

            var appartment1 = new Appartment(10, 10);

            registry.Register(appartment1);

            Assert.AreEqual(appartment1, registry.VacantBuildings.Single());


            var a = new DummyPerson();
            var b = new DummyPerson();
            var c = new DummyPerson();
            appartment1.Housings[0].Join(a);
            Assert.AreEqual(appartment1, registry.VacantBuildings.Single());
            appartment1.Housings[1].Join(b);


            //people have joined the only building's two housings: no building should be available anymore
            Assert.IsFalse(registry.VacantBuildings.Any());


            appartment1.Housings[1].Leave(b);
            Assert.AreEqual(appartment1, registry.VacantBuildings.Single());


            var appartment2 = new Appartment(2);
            registry.Register(appartment2);

            Assert.AreEqual(2, registry.VacantBuildings.Count);

            appartment1.Housings[1].Join(b);

            Assert.AreEqual(appartment2, registry.VacantBuildings.Single());

            appartment2.Housings[0].Join(c);

            Assert.IsFalse(registry.VacantBuildings.Any());
            appartment2.Housings[0].Leave(c);
            Assert.AreEqual(appartment2, registry.VacantBuildings.Single());

            registry.Remove(appartment2);
            Assert.IsFalse(registry.VacantBuildings.Any());

        }
    }

    internal class DummyPerson : IPerson
    {
        public IReadOnlyCollection<SportType> Sports => throw new System.NotImplementedException();

        public IReadOnlyCollection<JobType> JobTypes => throw new System.NotImplementedException();

        public IReadOnlyCollection<HobbyType> HobbyTypes => throw new System.NotImplementedException();
    }
}
