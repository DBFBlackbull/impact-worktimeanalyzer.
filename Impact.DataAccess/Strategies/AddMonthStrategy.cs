using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddMonthStrategy : IAddRegistrationStrategy<Month>
    {
        private Dictionary<string, Month> Months { get; }

        public AddMonthStrategy(DateTime hireDate)
        {
            Months = new Dictionary<string, Month>();
            var dateTime = new DateTime(hireDate.Year, hireDate.Month, 1);
            while (dateTime < DateTime.Now)
            {
                Months[$"{dateTime.Year}{dateTime.Month}"] = new Month(dateTime);
                dateTime = dateTime.AddMonths(1);
            }
        }

        public void AddRegistration(WorkUnitFlat registration)
        {
            switch (registration.TaskName)
            {
                case "Fed torsdag":
                case "Fed Torsdag i SD":
                case "PL Fed Torsdag":
                    Months[$"{registration.Date.Year}{registration.Date.Month}"].AwesomeThursdayRawHours += registration.Hours;
                    break;
                case "R&D":
                    Months[$"{registration.Date.Year}{registration.Date.Month}"].RAndDRawHours = registration.Hours;
                    break;
                default:
                    return;
            }
        }

        public IEnumerable<Month> GetList()
        {
            var months = Months.Values.ToList();
            months.ForEach(m => m.ConvertToDecimal());
            return months.OrderBy(m => m.Date);
        }
    }
}