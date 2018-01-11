using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Contants;
using Impact.Core.Model;

namespace Impact.Business.Holiday
{
    public class HolidayService : IHolidayService
    {
        public IEnumerable<DateTime> CalculateHolidays(Quarter quarter)
        {
            var year = quarter.MidDate.Year;
            switch (quarter.Number)
            {
                case 1 :
                    return new List<DateTime>(EasterBasedHolidays(quarter)) { new DateTime(year, 1, 1), };
                case 2 :
                    return EasterBasedHolidays(quarter);
                case 3 :
                    return new List<DateTime>();
                case 4 :
                    return new List<DateTime>
                    {
                        new DateTime(year, 12, 24),
                        new DateTime(year, 12, 25),
                        new DateTime(year, 12, 26),
                        new DateTime(year, 12, 31),
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(quarter.Number), "There are only 4 quarters in a year dummy!");
            }
        }

        public void AddHolidayHours(Quarter quarter, IEnumerable<Week> weeks)
        {
            var holidays = CalculateHolidays(quarter).ToList();
            foreach (var week in weeks)
            {
                foreach (var date in week.Dates)
                {
                    if (holidays.Contains(date))
                        week.HolidayHours += ApplicationConstants.NormalWorkDay;
                }
            }
        }

        private List<DateTime> EasterBasedHolidays(Quarter quarter)
        {
            var year = quarter.MidDate.Year;
            var easter = CalculateEaster(year);

            var easterBasedHolidays = new List<DateTime>
            {
                easter.AddDays(-3), //Skærtorsdag
                easter.AddDays(-2), //Langfredag
                easter.AddDays(1),  //2. Påskedag
                easter.AddDays(26), //Store Bededag
                easter.AddDays(39), //Kristi Himmelfart
                easter.AddDays(50), //2. Pinsedag
            };
            easterBasedHolidays.RemoveAll(d => d < quarter.From || d > quarter.To);
            return easterBasedHolidays;
        }

        /// <summary>
        /// Black magic from the depth of the internet and stack overflow.
        /// Pray to the gods that this algorithm is correct.
        /// </summary>
        /// <param name="year">The year for which easter is to be calculated</param>
        /// <returns>The date that easter falls on</returns>
        private DateTime CalculateEaster(int year)
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