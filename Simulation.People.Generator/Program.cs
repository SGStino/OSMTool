using Simulation.Buildings;
using Simulation.Buildings.Types;
using Simulation.Buildings.Types.Facilities;
using Simulation.Data;
using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Simulation.People.Generator
{
    class Program
    {

        public static MultiDictionary<SportType, Building> sportLocations = new MultiDictionary<SportType, Building>();
        private static void add(Building loc)
        {
            if (loc is ISportLocation sportLoc)
                sportLocations.Add(sportLoc.SportProvider.AvailableSportTypes, loc);
            else if (loc is IFacilityProvider facilityProvider)
                foreach (var facility in facilityProvider.Facilities)
                    if (facility is ISportLocation sportFacility)
                        sportLocations.Add(sportFacility.SportProvider.AvailableSportTypes, loc);
        }
        static void Main(string[] args)
        {
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
            var generator = new PeopleGenerator();
            var profiles = new HashSet<Profile>();

            //for(int i = 0; i < 30000000; i++)
            //{
            //    profiles.Add(generator.Current);
            //    generator.Next();
            //}
            int count = 0;
            while (generator.Iteration < 1)
            {
                generator.Next();
                count++;
            }

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
