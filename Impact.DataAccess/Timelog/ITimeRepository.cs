using System;
using System.Collections.Generic;
using Impact.Core.Model;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public interface ITimeRepository
    {
        IEnumerable<Week> GetRawWeeksInQuarter(Quarter quarter, Profile profile, SecurityToken token);
        IEnumerable<Month> GetAwesomeThursdays(DateTime hiredDate, SecurityToken token);
        IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, Profile profile, SecurityToken token);
        IEnumerable<TimeRegistration> GetRegistrationsWithJiraId(string jiraId, int projectId, DateTime from, DateTime to, Profile profile, SecurityToken token);
        IDictionary<int, string> GetProject(string initials, SecurityToken token);
    }
}
