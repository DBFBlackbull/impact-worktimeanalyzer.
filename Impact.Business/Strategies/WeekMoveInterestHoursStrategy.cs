using Impact.Core.Model;

namespace Impact.Business.Strategies
{
    public class WeekMoveInterestHoursStrategy : IMoveHoursStrategy<Week>
    {
        public bool MoveHours(Week lowHoursElement, Week movableHoursElement)
        {
            var missingHours = lowHoursElement.NormalWorkWeek - (lowHoursElement.WorkHours + lowHoursElement.HolidayHours + lowHoursElement.QuarterEdgeHours);
            if (missingHours > movableHoursElement.InterestHours)
            {
                lowHoursElement.WorkHours += movableHoursElement.InterestHours;
                movableHoursElement.InterestHours = 0;
                return false;
            }

            lowHoursElement.WorkHours += missingHours;
            movableHoursElement.InterestHours -= missingHours;
            return true;
        }
    }
}