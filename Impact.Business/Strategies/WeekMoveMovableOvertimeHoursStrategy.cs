using Impact.Core.Model;

namespace Impact.Business.Strategies
{
    public class WeekMoveMovableOvertimeHoursStrategy : IMoveHoursStrategy<Week>
    {
        private readonly decimal _normalWorkWeek;

        public WeekMoveMovableOvertimeHoursStrategy(decimal normalWorkWeek)
        {
            _normalWorkWeek = normalWorkWeek;
        }

        public bool MoveHours(Week lowHoursElement, Week movableHoursElement)
        {
            var missingHours = _normalWorkWeek - (lowHoursElement.WorkHours + lowHoursElement.HolidayHours + lowHoursElement.QuarterEdgeHours);
            if (missingHours > movableHoursElement.MovableOvertimeHours)
            {
                lowHoursElement.WorkHours += movableHoursElement.MovableOvertimeHours;
                movableHoursElement.MovableOvertimeHours = 0;
                return false;
            }

            lowHoursElement.WorkHours += missingHours;
            movableHoursElement.MovableOvertimeHours -= missingHours;
            return true;
        }
    }
}