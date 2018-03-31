using Simulation.Buildings;
using Simulation.Buildings.Types;
using Simulation.Buildings.Types.Facilities;
using Simulation.Data;
using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Simulation.People.Generator
{
    class Program
    {

        //public static MultiDictionary<SportType, Building> sportLocations = new MultiDictionary<SportType, Building>();

        private static SportLocationRegistry sportLocations = new SportLocationRegistry();
        private static ResidentialBuildingRegistry residences = new ResidentialBuildingRegistry();



        private static void add(Building loc)
        {
            if (loc is ISportLocation sportLoc)
                sportLocations.Register(sportLoc);
            else if (loc is IFacilityProvider facilityProvider)
                foreach (var facility in facilityProvider.Facilities)
                    if (facility is ISportLocation sportFacility)
                        sportLocations.Register(sportFacility);
        }
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                add(new Stadion());
                add(new WaterSportCenter());
                add(new SwimmingPool());
                add(new Marina());
                add(new SportsHall());
                add(new FitnessCenter());
                add(new EquestrianCenter());
                add(new IceRink());
                add(new CommercialBuilding(new FitnessStudio()));
                add(new CommercialBuilding(new BoxingClub()));
                add(new SportsCenter());
                add(new CricketField());
                add(new GolfCourse());
            }

            var random = new Random(10);

            //for (int i = 0; i < 1000; i++)
            //{
            //    var appartmentCount = random.Next(1, 50);
            //    var sizes = new int[appartmentCount];
            //    Array.Fill(sizes, 6);
            //    var b = new Appartment(sizes);
            //    residences.Register(b);
            //}

            var generator = new PeopleGenerator();
            var profiles = new HashSet<Profile>();





            //for(int i = 0; i < 30000000; i++)
            //{
            //    profiles.Add(generator.Current);
            //    generator.Next();
            //}
            generator.Next();
            var sw = Stopwatch.StartNew();
            while (generator.Iteration < 50)
            {
                bool foundBuilding = findBuilding(random, generator);
                if (!foundBuilding)
                {
                    var appartmentCount = random.Next(1, 50);
                    var sizes = new int[appartmentCount];
                    Array.Fill(sizes, 6);
                    var b = new Appartment(sizes);
                    residences.Register(b);
                    findBuilding(random, generator);
                }
                //var location = sportLocations[generator.Current.Sport];
                //var loc = location.FirstOrDefault();
                //if (loc != null)
                //{
                //    if (loc.SportProvider.Join(generator.Current.Sport))
                //    {
                //        Console.WriteLine("joined " + generator.Current.Sport);
                //        continue;
                //    }
                //}
                //Console.WriteLine("No location found for " + generator.Current.Sport);
            }

            sw.Stop();
            var people = residences.Buildings.Sum(t => t.Housings.Inhabitants);

            Console.WriteLine("housing " + people + " persons in " + residences.Buildings.Count + " buildings took " + sw.Elapsed);

            generator.Random();
            printNext(generator.Current);
            while (true)
            {
                var answer = Console.ReadKey(true);

                switch (answer.KeyChar)
                {
                    case '1':
                        generator.Accept();
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("+");
                        Console.ResetColor();
                        printNext(generator.Current);
                        break;
                    case '2':
                        generator.Random();
                        printNext(generator.Current);
                        break;
                    case '3':
                        generator.Deny();
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("-");
                        Console.ResetColor();
                        printNext(generator.Current);
                        break;
                    default:
                        continue;
                }
            }
        }

        private static bool findBuilding(Random random, PeopleGenerator generator)
        {
            var vacantBuildings = residences.VacantBuildings;
            bool foundBuilding = false; ;
            foreach (var vacantBuilding in vacantBuildings)
            {
                var housing = vacantBuilding.Housings.First(t => t.IsVacant());
                var size = random.Next(1, 5);
                if (housing.Capacity > size)
                {
                    foundBuilding = true;
                    for (int i = 0; i < size; i++)
                    {
                        housing.Join(new Person(generator.Current));
                        generator.Next();
                    }
                    break;
                }
            }

            return foundBuilding;
        }

        private static void printNext(Profile current)
        {
            Console.WriteLine();
            var maxHobbyLen = PeopleGenerator.Hobbies.Max(t => t.ToString().Length);
            var maxJobLen = PeopleGenerator.Jobs.SelectMany(t => Enumerable.Range(0, t.GetMaxLevel() + 1).Select(n => t.GetJobName(n))).Max(t => t.Length);
            var maxSportsLen = PeopleGenerator.Sports.Max(t => t.ToString().Length);
            Console.Write(current.Job.GetJobName(current.JobLevel).PadRight(maxJobLen + 2) + current.Hobby.ToString().PadRight(maxHobbyLen + 2) + current.Sport.ToString().PadRight(maxSportsLen));

            var location = string.Join(", ", sportLocations[current.Sport].Select(t => t?.GetType().Name)).PadRight(50);

            Console.Write(location.PadRight(50));

            var r = current.Evaluate();
            if (r.HasValue)
            {
                if (r.Value)
                {
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("+");
                    Console.ResetColor();
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("-");
                    Console.ResetColor();
                }
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write("0");
                Console.ResetColor();
            }
        }
    }

    internal class Person : IPerson
    {
        private Profile profile;

        public Person(Profile current)
        {
            this.profile = current;
        }

        public IReadOnlyCollection<SportType> Sports => new[] { profile.Sport };

        public IReadOnlyCollection<JobType> JobTypes => new[] { profile.Job };

        public IReadOnlyCollection<HobbyType> HobbyTypes => new[] { profile.Hobby };
    }

    internal class Profile
    {
        public Profile(JobType job, int jobLevel, SportType sport, HobbyType hobby)
        {
            Job = job;
            JobLevel = jobLevel;
            Sport = sport;
            Hobby = hobby;
        }

        public JobType Job { get; }
        public int JobLevel { get; }
        public SportType Sport { get; }
        public HobbyType Hobby { get; }

        public override int GetHashCode()
        {
            return Job.GetHashCode() ^ JobLevel.GetHashCode() ^ Sport.GetHashCode() ^ Hobby.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Profile prof)
            {
                return prof.Job == Job && prof.JobLevel == JobLevel && prof.Sport == Sport && prof.Hobby == Hobby;
            }

            return base.Equals(obj);
        }
    }
}
