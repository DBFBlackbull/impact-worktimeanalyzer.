using System;
using System.Collections.Generic;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Time
{
	public interface ITimeService
	{
		bool IsAuthorized(string username, string password, out SecurityToken securityToken);
	    Quarter GetQuarter(DateTime dateTime);
	    IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken securityToken);
	    IEnumerable<Week> GetNormalizedWeeks(List<Week> weeksList);
	}
}
