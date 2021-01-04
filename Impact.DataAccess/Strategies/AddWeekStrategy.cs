using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddWeekStrategy : IAddRegistrationStrategy<Week>
    {
        private readonly IDictionary<DateTime, decimal> _workingHours;
        private ConcurrentDictionary<int, Week> Weeks { get; }
        
        public AddWeekStrategy(IDictionary<DateTime, decimal> workingHours)
        {
            _workingHours = workingHours;
            Weeks = new ConcurrentDictionary<int, Week>();
        }

        public void AddRegistration(WorkUnitFlat registration)
        {
            var dateTime = registration.Date;
            var weekNumber = ApplicationConstants.GetWeekNumber(dateTime);

            if (!Weeks.TryGetValue(weekNumber, out var week))
                Weeks[weekNumber] = week = CreateWeek(weekNumber, dateTime);

            week.TotalHours += Convert.ToDecimal(registration.Hours);
        }

        public IEnumerable<Week> GetList()
        {
            return Weeks.Values.OrderBy(w => w.Dates.Keys.First()).ToList();
        }
        
        private Week CreateWeek(int weekNumber, DateTime dateTime)
        {
            var day = dateTime.BackTo(DayOfWeek.Monday);
            
            var week = new Week { Number = weekNumber };
            // Runs through the interval [Monday;Friday]
            while (day.DayOfWeek < DayOfWeek.Saturday)
            {
                week.Dates.Add(day, _workingHours[day]);
                day = day.AddDays(1);
            }
            
            week.NormalWorkWeek = week.Dates.Values.Sum(d => d).Normalize();
            
            return week;
        }
    }
}