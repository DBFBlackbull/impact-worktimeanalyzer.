using System;
using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Business.Scanner
{
    public class MissingHoursScannerService : IHoursScannerService
    {
        public IEnumerable<DateTime> ScanWeeks(IEnumerable<Week> weeks)
        {
            var daysWithoutHours = new List<DateTime>();
            
            foreach (var week in weeks)
            {
//                foreach (var dateToHours in week.HoursPerDay)
//                {
//                    if (dateToHours.Value == 0 && dateToHours.Key.DayOfWeek != DayOfWeek.Saturday && dateToHours.Key.DayOfWeek != DayOfWeek.Sunday)
//                        daysWithoutHours.Add(dateToHours.Key);
//                }
            }

            return daysWithoutHours;
        }
    }
}