using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Constants;
using Impact.Core.Model;

namespace Impact.Business.Holiday
{
    public class HolidayService : IHolidayService
    {
        public void AddHolidayHours(Quarter quarter, IEnumerable<Week> weeks)
        {
            var holidays = GetHolidays(quarter.From, quarter.To);
            foreach (var week in weeks)
            {
                foreach (var date in week.Dates)
                {
                    if (holidays.ContainsKey(date.Key))
                        week.HolidayHours += date.Value;
                }
            }
        }

        public IEnumerable<VacationDay> GetHolidays(VacationYear vacationYear)
        {
            var dateTimes = GetHolidays(vacationYear.StartDate, vacationYear.EndDate);
            return dateTimes.Select(kv => new VacationDay(kv.Key, 1, kv.Value));
        }

        private static Dictionary<DateTime, string> GetHolidays(DateTime from, DateTime to)
        {
            var holidays = new Dictionary<DateTime, string>();
            foreach (var year in new HashSet<int> {from.Year, to.Year})
            {
                holidays.Add(new DateTime(year, 1, 1), "Nytårsdag"); //Nytårsdag
                
                var easter = CalculateEaster(year);
                holidays.Add(easter.AddDays(-3), "Skærtorsdag");
                holidays.Add(easter.AddDays(-2), "Langfredag");
                holidays.Add(easter.AddDays(1),  "Påskedag");
                holidays.Add(easter.AddDays(26), "Store Bededag");
                holidays.Add(easter.AddDays(39), "Kristi Himmelfart");
                holidays.Add(easter.AddDays(50), "2. Pinsedag");

//                holidays.Add(new DateTime(year, 12, 24)); // Christmas is not a holiday without a union settlement
                holidays.Add(new DateTime(year, 12, 25), "1. Juledag");
                holidays.Add(new DateTime(year, 12, 26), "2. Juledag");
//                holidays.Add(new DateTime(year, 12, 31)); // New years eve is not a holiday without a union settlement
            }

            return holidays
                .Where(kv => from <= kv.Key && kv.Key <= to)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Black magic from the depth of the internet and stack overflow.
        /// Pray to the gods that this algorithm is correct.
        /// </summary>
        /// <param name="year">The year for which easter is to be calculated</param>
        /// <returns>The date that easter falls on</returns>
        private static DateTime CalculateEaster(int year)
        {
            int g = year % 19;
            int c = year / 100;
            int h = h = (c - (int)(c / 4) - (int)((8 * c + 13) / 25) 
                         + 19 * g + 15) % 30;
            int i = h - (int)(h / 28) * (1 - (int)(h / 28) * 
                                         (int)(29 / (h + 1)) * (int)((21 - g) / 11));

            int day = i - ((year + (int)(year / 4) + 
                            i + 2 - c + (int)(c / 4)) % 7) + 28;
            int month = 3;

            if (day > 31)
            {
                month++;
                day -= 31;
            }
            
            return new DateTime(year, month, day);
        }
    }
}