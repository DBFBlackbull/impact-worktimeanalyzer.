using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken token)
        {
            var instanceProjectManagementClient = ProjectManagementHandler.Instance.ProjectManagementClient;
            var result = instanceProjectManagementClient.GetWorkPaged("deb", quarter.From, quarter.To, 1, 500, token);

            var calendar = CultureInfo.InvariantCulture.Calendar;
            var weeksToHoursDictionary = new Dictionary<int, Week>();
            
            if (result.ResponseState != ExecutionStatus.Success) 
                return weeksToHoursDictionary.Values;
            
            var workUnitFlats = result.Return;
            foreach (var workUnitFlat in workUnitFlats)
            {
                var dateTime = workUnitFlat.Date;
                var weekNumber = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

                if (!weeksToHoursDictionary.TryGetValue(weekNumber, out var week))
                    weeksToHoursDictionary[weekNumber] = week = CreateWeek(weekNumber, dateTime);

                week.TotalHours += workUnitFlat.Hours;
            }

            return weeksToHoursDictionary.Values.OrderBy(w => w.Number);;
        }

        private static Week CreateWeek(int weekNumber, DateTime dateTime)
        {
            var day = dateTime;
            while (day.DayOfWeek != DayOfWeek.Monday)
            {
                day = day.AddDays(-1);
            }
            var week = new Week { Number = weekNumber };
            week.Dates.Add(day);
            week.Dates.Add(day.AddDays(1));
            week.Dates.Add(day.AddDays(2));
            week.Dates.Add(day.AddDays(3));
            week.Dates.Add(day.AddDays(4));
            return week;
        }
    }
}
