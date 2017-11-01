using System;
using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Business.Time
{
	public interface ITimeService
	{
		bool IsAuthorized(string username, string password);
	    Quarter GetQuarter(DateTime dateTime = new DateTime());
	    IEnumerable<Week> GetWeeksInQuarter(Quarter quarter);
	}
}
