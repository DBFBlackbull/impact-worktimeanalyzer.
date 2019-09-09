using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Constants;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddWeekStrategy : IAddRegistrationStrategy<Week>
    {
        private Dictionary<int, Week> Weeks { get; }
        
        public AddWeekStrategy()
        {
            Weeks = new Dictionary<int, Week>();
        }

        public void AddRegistration(WorkUnitFlat registration)
        {
            var dateTime = registration.Date;
            var weekNumber = ApplicationConstants.GetWeekNumber(dateTime);

            if (!Weeks.TryGetValue(weekNumber, out var week))
                Weeks[weekNumber] = week = CreateWeek(weekNumber, dateTime);

            week.TotalHours += registration.Hours;
        }

        public IEnumerable<Week> GetList()
        {
            return Weeks.Values.OrderBy(w => w.Number).ToList();
        }
        
        private static Week CreateWeek(int weekNumber, DateTime dateTime)
        {
            var day = dateTime;
            while (day.DayOfWeek != ApplicationConstants.DanishFirstDayOfWeek)
            {
                day = day.AddDays(-1);
            }
            var week = new Week { Number = weekNumber };
            week.Dates.Add(day);                  //Monday
            week.Dates.Add(day.AddDays(1)); //Tuesday
            week.Dates.Add(day.AddDays(2)); //Wednesday
            week.Dates.Add(day.AddDays(3)); //Thursday
            week.Dates.Add(day.AddDays(4)); //Friday
            return week;
        }
    }
}