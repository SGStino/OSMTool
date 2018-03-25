using Simulation.Leisure;
using Simulation.Leisure.Sports;
using Simulation.Occupation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simulation.People.Generator
{
    public class PeopleGenerator
    {
        public static IReadOnlyList<JobType> Jobs { get; } = Enum.GetValues(typeof(JobType)).Cast<JobType>().ToArray();
        public static IReadOnlyList<SportType> Sports { get; } = Enum.GetValues(typeof(SportType)).Cast<SportType>().ToArray();
        public static IReadOnlyList<HobbyType> Hobbies { get; } = Enum.GetValues(typeof(HobbyType)).Cast<HobbyType>().ToArray();

        Random rnd = new Random();
        internal Profile Current { get; set; }

        public void Accept()
        {
            Next();
        }

        public void Next()
        {
            var job = pickRandom(Jobs);
            var maxLevel = job.GetMaxLevel();
            var level = (int)(rnd.NextDouble() * rnd.NextDouble() * maxLevel);
            if (level > maxLevel) level = maxLevel;
            Current = new Profile(job, level, pickRandom(Sports), pickRandom(Hobbies));
        }

        private T pickRandom<T>(IReadOnlyList<T> list)
        {
            var i = rnd.Next(0, list.Count);
            if (i >= list.Count) i = list.Count - 1;
            return list[i];
        }

        public void Deny()
        {
            Next();
        }
    }
}
