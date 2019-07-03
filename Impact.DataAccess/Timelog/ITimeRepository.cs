using System;
using System.Collections.Generic;
using Impact.Core.Model;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public interface ITimeRepository
    {
        IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken token);
        IEnumerable<Month> GetAwesomeThursdays(DateTime hireDate, SecurityToken token);
        IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, SecurityToken token);
    }
}
