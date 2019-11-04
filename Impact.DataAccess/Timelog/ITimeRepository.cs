using System;
using System.Collections.Generic;
using Impact.Core.Model;
using TimeLog.ReportingApi.SDK;
using Project = Impact.Core.Model.Project;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public interface ITimeRepository
    {
        IEnumerable<Week> GetRawWeeksInQuarter(Quarter quarter, Profile profile, SecurityToken token);
        IEnumerable<Month> GetAwesomeThursdays(DateTime hiredDate, SecurityToken token);
        IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, Profile profile, SecurityToken token);
        IEnumerable<TimeRegistration> GetRegistrationsWithJiraId(string jiraId, DateTime from, DateTime to, Profile profile, SecurityToken token);
        IEnumerable<Project> GetTasks(string initials, SecurityToken token);
    }
}
