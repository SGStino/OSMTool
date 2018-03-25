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

        static void Main(string[] args)
        {
            var generator = new PeopleGenerator();
            generator.Next();
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
                        generator.Next();
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
    }
}
