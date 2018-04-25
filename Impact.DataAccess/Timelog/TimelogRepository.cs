using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Contants;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken token)
        {
            var instanceProjectManagementClient = ProjectManagementHandler.Instance.ProjectManagementClient;
            var result = instanceProjectManagementClient.GetWorkPaged(token.Initials, quarter.From, quarter.To, 1, 500, token);

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

        public IEnumerable<Month> GetAwesomeThursdays(SecurityToken token)
        {
            var dateToMonth = new Dictionary<DateTime, Month>();

            var instanceProjectManagementClient = ProjectManagementHandler.Instance.ProjectManagementClient;
            var result = instanceProjectManagementClient.GetWorkPaged(token.Initials, new DateTime(2012, 1, 1), DateTime.Now, 1, 1, token);
            DateTime firstRecord = result.Return.Min(w => w.Date);

            while (firstRecord < DateTime.Now)
            {
                var dateTime = new DateTime(firstRecord.Year, firstRecord.Month, 1);
                dateToMonth[dateTime] = new Month(dateTime);
                firstRecord = firstRecord.AddMonths(1);
            }
            
            WorkUnitFlat[] workUnitFlats;
            var pageIndex = 1;
            do
            {
                result = instanceProjectManagementClient.GetWorkPaged(token.Initials, new DateTime(2012, 1, 1), DateTime.Now, pageIndex, ApplicationConstants.PageSize, token);
                if (result.ResponseState != ExecutionStatus.Success)
                    break;
                
                workUnitFlats = result.Return;
                
                var awesomeThursdays = workUnitFlats.Where(w => w.TaskName == "Fed torsdag" || w.TaskName == "PL Fed Torsdag").ToList();
                foreach (var awesomeThursday in awesomeThursdays)
                {
                    var dateTime = new DateTime(awesomeThursday.Date.Year, awesomeThursday.Date.Month, 1);
                    if (!dateToMonth.TryGetValue(dateTime, out var month))
                        dateToMonth[dateTime] = month = new Month(dateTime);

                    month.RegisteredHours += awesomeThursday.Hours;
                }

                pageIndex++;
            } while (workUnitFlats.Length == ApplicationConstants.PageSize);
            
            dateToMonth.Values.ToList().ForEach(m => m.ConvertToDecimal());
            return dateToMonth.Values.OrderBy(m => m.Date);
        }
    }
}
