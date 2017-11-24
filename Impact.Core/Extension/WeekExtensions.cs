using System;
using Impact.Core.Contants;
using Impact.Core.Model;

namespace Impact.Core.Extension
{
    public static class WeekExtensions
    {
        // So ugly please find a better solution. Really really
        public static Week CategorizeHours(this Week week)
        {
            var hours = Math.Round(Convert.ToDecimal(week.TotalHours), 2);

            var workHoursMinusHolidayHours = ApplicationConstants.NormalWorkWeek - week.HolidayHours;
            if (hours >= workHoursMinusHolidayHours)
            {
                week.WorkHours = workHoursMinusHolidayHours;
                hours -= workHoursMinusHolidayHours;
            }
            else
            {
                week.WorkHours = hours;
                return week;
            }

            if (hours >= ApplicationConstants.InterestConst)
            {
                week.InterestHours = ApplicationConstants.InterestConst;
                hours -= ApplicationConstants.InterestConst;
            }
            else
            {
                week.InterestHours = hours;
                return week;
            }

            if (hours >= ApplicationConstants.MoveableConst)
            {
                week.MoveableOvertimeHours = ApplicationConstants.MoveableConst;
                hours -= ApplicationConstants.MoveableConst;
            }
            else
            {
                week.MoveableOvertimeHours = hours;
                return week;
            }

            week.LockedOvertimeHours = hours;

            return week;
        }
    }
}