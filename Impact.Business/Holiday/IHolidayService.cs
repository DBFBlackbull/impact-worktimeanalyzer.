using System.Collections.Generic;
using Impact.Core.Model;

namespace Impact.Business.Holiday
{
    public interface IHolidayService
    {
        void AddHolidayHours(Quarter quarter, IEnumerable<Week> weeks);
    }
}