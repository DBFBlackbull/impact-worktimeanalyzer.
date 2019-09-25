using Impact.Core.Constants;
using Impact.Core.Model;

namespace Impact.Business.Strategies
{
    public class MonthMoveHoursStrategy : IMoveHoursStrategy<Month>
    {
        public bool MoveHours(Month lowHoursElement, Month movableHoursElement)
        {
            if (lowHoursElement.Date < ApplicationConstants.RAndDStartDate)
            {
                var movableAwesomeHours = movableHoursElement.AwesomeThursdayHours - ApplicationConstants.AwesomeThursdayApproximation;
                var missingAwesomeHours = ApplicationConstants.AwesomeThursdayApproximation - lowHoursElement.AwesomeThursdayHours;
                
                if (missingAwesomeHours > movableAwesomeHours)
                {
                    lowHoursElement.AwesomeThursdayHours += movableAwesomeHours;
                    movableHoursElement.AwesomeThursdayHours -= movableAwesomeHours;
                    return false;
                }
                
                lowHoursElement.AwesomeThursdayHours += missingAwesomeHours;
                movableHoursElement.AwesomeThursdayHours -= missingAwesomeHours;
                return true;
            }
            
            var movableHours = (movableHoursElement.AwesomeThursdayHours + movableHoursElement.RAndDHours) - ApplicationConstants.AwesomeThursdayApproximation;
            var missingHours = ApplicationConstants.AwesomeThursdayApproximation - (lowHoursElement.AwesomeThursdayHours + lowHoursElement.RAndDHours);
            
            if (missingHours > movableHours)
            {
                lowHoursElement.RAndDHours += movableHours;
                movableHoursElement.RAndDHours -= movableHours;
                return false;
            }

            lowHoursElement.RAndDHours += missingHours;
            movableHoursElement.RAndDHours -= missingHours;
            return true;
        }
    }
}