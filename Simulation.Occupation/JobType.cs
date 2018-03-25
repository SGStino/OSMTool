using System;
using System.Collections.Generic;
using static Simulation.Occupation.Income;
using static Simulation.Occupation.JobType;
using static Simulation.Occupation.EducationLevel;
namespace Simulation.Occupation
{
    public enum Income
    {
        None,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum EducationLevel
    {
        Uneducated,
        BasicEducation,
        SecondaryEducation,
        Bachelor,
        Master,
        Doctorate
    }

    public enum JobType
    {
        Teacher,
        Politician,
        Legal,
        LawEnforcement,
        FireFighter,
        Manager,
        Driver,
        Farmer,
        BlueCollar,
        WhiteCollar,
        Nurse,
        Waiter,
        Doctor
    }

    public static class JobMatrix
    {
        public static int GetMaxLevel(this JobType type)
        {
            int maxLevel = 0; 
            if (jobNames.TryGetValue(type, out var names))
                maxLevel = Math.Max(maxLevel, names.Length);
            if (incomes.TryGetValue(type, out var incs))
                maxLevel = Math.Max(maxLevel, incs.Length);
            if (incomes.TryGetValue(type, out var educationLevels))
                maxLevel = Math.Max(maxLevel, educationLevels.Length);
            return maxLevel;
        }

        private static Dictionary<JobType, Income[]> incomes = new Dictionary<JobType, Income[]>
       {
            { Teacher, new [] { Medium, Medium, Medium, High, High } },
            { Legal, new []{ Medium, High, VeryHigh} },
            { LawEnforcement, new []{ Low, Medium, Medium} },
            { FireFighter, new []{ Low, Medium, Medium, High, High} },
            { Manager, new []{ Medium, High, VeryHigh} },
            { Driver, new [] { Medium } },
            { Farmer, new []{ VeryLow, Low} },
            { Nurse, new [] { Medium, Medium, High, High} },
            { Doctor, new []{ Medium, High, VeryHigh }  },
            { BlueCollar, new [] { VeryLow, Low, Medium, High} },
            { WhiteCollar, new [] { Low, Medium, High, VeryHigh} },
       };

        private static Dictionary<JobType, EducationLevel[]> educationLevels = new Dictionary<JobType, EducationLevel[]>()
        {
            { Teacher, new [] { EducationLevel.Bachelor, EducationLevel.Bachelor, EducationLevel.Bachelor, EducationLevel.Master, EducationLevel.Doctorate } },
            { Legal, new []{ EducationLevel.Bachelor, EducationLevel.Master, EducationLevel.Master} },
            { LawEnforcement, new []{ EducationLevel.BasicEducation, EducationLevel.SecondaryEducation, EducationLevel.Bachelor} },
            { FireFighter, new []{ EducationLevel.BasicEducation, EducationLevel.SecondaryEducation} },
            { Manager, new []{ EducationLevel.SecondaryEducation, EducationLevel.Bachelor, EducationLevel.Master} },
            { Driver, new [] { EducationLevel.SecondaryEducation } },
            { Farmer, new []{ EducationLevel.BasicEducation, EducationLevel.Bachelor} },
            { Nurse, new [] {EducationLevel.Bachelor, EducationLevel.Bachelor, EducationLevel.Master} },
            { Doctor, new []{ EducationLevel.Master, EducationLevel.Master, EducationLevel.Doctorate }  },
            { BlueCollar, new [] { EducationLevel.BasicEducation, EducationLevel.SecondaryEducation, EducationLevel.Bachelor, EducationLevel.Master} },
            { WhiteCollar, new [] { EducationLevel.SecondaryEducation, EducationLevel.Bachelor, EducationLevel.Master, EducationLevel.Doctorate} },
        };

        private static Dictionary<JobType, string[]> jobNames = new Dictionary<JobType, string[]>()
        {
            { Teacher, new []{"Daycare Teacher", "Elementary Teacher", "Highschool Teacher", "College Teacher", "University Professor" } },
            { Politician,  new []{"District Council", "City Council", "Regional Council", "Government" } },
            { Legal, new []{ "Notary", "Lawyer", "Judge"} },
            { LawEnforcement,new []{ "Police Officer", "Police Constable", "Police Superintendent", "Police Commissioner", "Police Chief Commisioner" } },
            { FireFighter, new [] { "FireFighter", "Fire Engineer", "Fire Luitenant", "Fire Captain", "Fire Chief" } },
            { Manager, new [] { "Store Manager", "Office Manager", "Company Mananger" } },
            { Driver, new [] { "Bus Driver", "Taxi Driver", "Private Driver" } },
            { Farmer, new []{ "Farmboy", "Farmer" } },
            { Nurse, new [] { "Home Nurse", "Geriatric Nuse", "Hospital Nurse", "Specialized Nurse" } },
            { Doctor, new []{ "General Practitioner", "Physician", "Doctor" } },
        };

        public static Income GetIncome(this JobType type, int level)
        {
            if (incomes.TryGetValue(type, out var jobIncomes) && jobIncomes.Length > 0)
            {
                if (level < 0) level = 0;
                if (level >= jobIncomes.Length) level = jobIncomes.Length - 1;
                return jobIncomes[level];
            }
            else
                return Medium;
        }

        public static EducationLevel GetJobEducationLevel(this JobType type, int level)
        {
            if (educationLevels.TryGetValue(type, out var levels) && levels.Length > 0)
            {
                if (level < 0) level = 0;
                if (level >= levels.Length) level = levels.Length - 1;
                return levels[level];
            }
            else
                return SecondaryEducation;
        }

        public static string GetJobName(this JobType type, int level)
        {
            if (jobNames.TryGetValue(type, out var names) && names.Length > 0)
            {
                if (level < 0) level = 0;
                if (level >= names.Length) level = names.Length - 1;
                return names[level];
            }
            else
                return type.ToString() + " " + (1 + level);
        }
    }
}
