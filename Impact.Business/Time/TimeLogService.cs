using System;
using System.Collections.Generic;
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

        public bool IsAuthorized(string username, string password, out SecurityToken securityToken)
        {
            var authorized = SecurityHandler.Instance.TryAuthenticate(username, password, out var messages);
            securityToken = ProjectManagementHandler.Instance.Token;
            return authorized;
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
                toDate = new DateTime(year, 3, 31);
                quarter.Number = 1;
            }
            else if (month < 7)
            {
                fromDate = new DateTime(year, 4, 1);
                midDate = new DateTime(year, 5, 15);
                toDate = new DateTime(year, 6, 30);
                quarter.Number = 2;
            }
            else if (month < 10)
            {
                fromDate = new DateTime(year, 7, 1);
                midDate = new DateTime(year, 8, 15);
                toDate = new DateTime(year, 9, 30);
                quarter.Number = 3;
            }
            else
            {
                fromDate = new DateTime(year, 10, 1);
                midDate = new DateTime(year, 11, 15);
                toDate = new DateTime(year, 12, 31);
                quarter.Number = 4;
            }

            quarter.From = fromDate;
            quarter.To = toDate;
            quarter.MidDate = midDate;

            return quarter;
        }

        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken securityToken)
        {
            return _timeRepository.GetWeeksInQuarter(quarter, securityToken);
        }
    }
}
