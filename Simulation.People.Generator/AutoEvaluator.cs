using Simulation.Leisure.Sports;
using Simulation.Occupation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Simulation.Leisure.Sports.SportType;
using static Simulation.Occupation.Income;
namespace Simulation.People.Generator
{
    internal static class AutoEvaluator
    {
        public static Income GetWealth(this SportType type) => (Income)sportWealth.Where(t => t.sport == type).Select(t => (int)t.wealth).DefaultIfEmpty(0).Average();

        public static List<(Income wealth, SportType sport)> sportWealth = new List<(Income wealth, SportType sport)>()
        {
            (None, Soccer),
            (High, Cricket),
            (Medium, Hockey),
            (High, Tennis),
            (Medium, Volleyball),
            (Low, TableTennis),
            (VeryHigh, Golf),
            (None, Basketball),
            (None, Rugby),
            (Medium, Swimming),
            (None, Boxing),
            (None, Wrestling),
            (VeryHigh, Equestrian),
            (High, Softball),
            (Medium, Squash),
            (Medium, Climbing),
            (Low, MartialArts),
            (Medium, Cycling),
            (VeryHigh, Sailing),
            (Medium, Archery),
            (High, Fencing),
            (Low, Fitness),
            (Medium, Yoga),
            (Medium, WaterPolo),
            (Medium, WaterAerobics),
            (Medium, Rowing),
            (High, WaterSki),
            (VeryHigh, Jetski),
            (VeryHigh, BoatRacing),
            (VeryHigh, Racing),
            (Low, IndoorSoccer),
            (Low, Gymnastics),
            (None, Jogging),
            (None, Running)
        };



        public static bool? Evaluate(this Profile profile)
        {
            var income = profile.Job.GetIncome(profile.JobLevel);

            var wealth = profile.Sport.GetWealth();

            var inc = (int)income;
            var wea = (int)wealth;

            var diff = Math.Abs(inc - wea);

            if (inc < wea)
            {
                if (diff > 2) return false;
                if (diff < 1) return true;
                return null;
            }
            else
            {
                if (diff > 4) return null;
                return true;
            }
        }

    }
}
