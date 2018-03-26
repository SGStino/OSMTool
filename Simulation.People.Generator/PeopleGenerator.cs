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


        private int jobIndex, jobLevelIndex, sportIndex, hobbyIndex;
        private int iteration;
        Random rnd = new Random();
        internal Profile Current { get; set; }
        public int Iteration => iteration;

        public void Accept()
        {
            Random();
        }

        public void Random()
        {
            jobIndex = pickRandom(Jobs.Count);
            var job = Jobs[jobIndex];
            jobLevelIndex = pickRandom(job.GetMaxLevel() + 1);
            sportIndex = pickRandom(Jobs.Count);
            hobbyIndex = pickRandom(Hobbies.Count);

            Current = new Profile(job, jobLevelIndex, Sports[sportIndex], Hobbies[hobbyIndex]);
        }

        private int pickRandom(int count)
        {
            var r = rnd.Next(0, count);
            return r < count ? r : count;
        }

        public void Next()
        {
            var job = Jobs[jobIndex];
            var maxLevel = job.GetMaxLevel();
            jobLevelIndex++;

            if (jobLevelIndex > maxLevel)
            {
                jobLevelIndex = 0;
                jobIndex++;
                if (jobIndex >= Jobs.Count)
                {
                    jobIndex = 0;
                    sportIndex++;
                    if (sportIndex >= Sports.Count)
                    {
                        sportIndex = 0;
                        hobbyIndex++;
                        if (hobbyIndex >= Hobbies.Count)
                        {
                            hobbyIndex = 0;
                            iteration++;
                        }
                    }
                }
            }
              
            Current = new Profile(job, jobLevelIndex, Sports[sportIndex], Hobbies[hobbyIndex]);
        }

        //private T pickRandom<T>(IReadOnlyList<T> list)
        //{
        //    var i = rnd.Next(0, list.Count);
        //    if (i >= list.Count) i = list.Count - 1;
        //    return list[i];
        //}

        public void Deny()
        {
            Random();
        }
    }
}
