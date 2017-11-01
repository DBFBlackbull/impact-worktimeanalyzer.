using System;
using System.Collections.Generic;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Time
{
    public class TimeLogService : ITimeService
    {
        private readonly ITimeRepository _timeRepository;

        public TimeLogService(ITimeRepository timeRepository)
        {
            _timeRepository = timeRepository;
        }

        public bool IsAuthorized(string username, string password)
        {
            IEnumerable<string> messages;
            return SecurityHandler.Instance.TryAuthenticate(username, password, out messages);
        }

        public Quarter GetQuarter(DateTime dateTime = new DateTime())
        {
            var quarter = new Quarter();

            var year = dateTime.Year;
            var month = dateTime.Month;

            DateTime fromDate;
            DateTime toDate;
            DateTime midDate;

            if (month < 4)
            {
                fromDate = new DateTime(year, 1, 1);
                midDate = new DateTime(year, 2, 15);
                toDate = new DateTime(year, 3, 1).LastDayInMonth();
                quarter.Number = 1;
            }
            else if (month < 7)
            {
                fromDate = new DateTime(year, 4, 1);
                midDate = new DateTime(year, 5, 15);
                toDate = new DateTime(year, 6, 1).LastDayInMonth();
                quarter.Number = 2;
            }
            else if (month < 10)
            {
                fromDate = new DateTime(year, 7, 1);
                midDate = new DateTime(year, 8, 15);
                toDate = new DateTime(year, 9, 1).LastDayInMonth();
                quarter.Number = 3;
            }
            else
            {
                fromDate = new DateTime(year, 10, 1);
                midDate = new DateTime(year, 11, 15);
                toDate = new DateTime(year, 12, 1).LastDayInMonth();
                quarter.Number = 4;
            }

            while (fromDate.DayOfWeek != DayOfWeek.Monday)
            {
                fromDate = fromDate.AddDays(-1);
            }

            while (toDate.DayOfWeek != DayOfWeek.Sunday)
            {
                toDate = toDate.AddDays(-1);
            }

            quarter.From = fromDate;
            quarter.To = toDate;
            quarter.MidDate = midDate;

            return quarter;
        }

        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter)
        {
            SecurityToken token;
            try
            {
                token = ProjectManagementHandler.Instance.Token;
            }
            catch (Exception)
            {
                return null;
            }

            return _timeRepository.GetWeeksInQuarter(quarter, token);
        }
    }
}
