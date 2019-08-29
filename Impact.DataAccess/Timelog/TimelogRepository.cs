using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Constants;
using Impact.Core.Model;
using Impact.DataAccess.Strategies;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        private static readonly ProjectManagementServiceClient Client = ProjectManagementHandler.Instance.ProjectManagementClient;
        private const int PageSize = 500;

        public IEnumerable<Week> GetRawWeeksInQuarter(Quarter quarter, SecurityToken token)
        {
            return GetWorkUnitsData<Week>(quarter.From, quarter.To, token, new AddWeekStrategy());
        }

        public IEnumerable<Month> GetAwesomeThursdays(DateTime hireDate, SecurityToken token)
        {
            return GetWorkUnitsData<Month>(hireDate, DateTime.Now, token, new AddMonthStrategy(hireDate));
        }

        public IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, SecurityToken token)
        {
            return GetWorkUnitsData<VacationDay>(from, to, token, new AddVacationDayStrategy(from, to));
        }

        private static IEnumerable<T> GetWorkUnitsData<T>(DateTime from, DateTime to, SecurityToken token, IAddRegistrationStrategy<T> strategy)
        {
            WorkUnitFlat[] registrations;
            var pageIndex = 1;
            do
            {
                var result = Client.GetWorkPaged(token.Initials, from, to, pageIndex, PageSize, token);
                if (result.ResponseState != ExecutionStatus.Success)
                    break;
                
                registrations = result.Return;

                foreach (var registration in registrations)
                    strategy.AddRegistration(registration);
                
                pageIndex++;
            } while (registrations.Length == PageSize);

            return strategy.GetList();
        }
    }
}
