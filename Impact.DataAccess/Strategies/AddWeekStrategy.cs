using System;
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
        private readonly Dictionary<DateTime, decimal> _workingHours;
        private Dictionary<int, Week> Weeks { get; }
        
        public AddWeekStrategy(Dictionary<DateTime, decimal> workingHours)
        {
            _workingHours = workingHours;
            Weeks = new Dictionary<int, Week>();
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
            return Weeks.Values.OrderBy(w => w.Number).ToList();
        }
        
        private Week CreateWeek(int weekNumber, DateTime dateTime)
        {
            var day = dateTime;
            while (day.DayOfWeek != DayOfWeek.Monday)
            {
                day = day.AddDays(-1);
            }
            var week = new Week { Number = weekNumber };
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