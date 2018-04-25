using System.Collections.Generic;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.SecurityService;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public interface ITimeRepository
    {
        IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken token);
        IEnumerable<Month> GetAwesomeThursdays();
    }
}
