using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Business.Holiday;
using Impact.Core.Contants;
using Impact.Core.Interfaces;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Time
{
    public class TimeLogService : ITimeService
    {
        private readonly ITimeRepository _timeRepository;
        private readonly IHolidayService _holidayService;

        public TimeLogService(ITimeRepository timeRepository, IHolidayService holidayService)
        {
            _timeRepository = timeRepository;
            _holidayService = holidayService;
        }

        public Quarter GetQuarter(DateTime dateTime)
        {
            var quarter = new Quarter();

            var year = dateTime.Year;
            var month = dateTime.Month;

            DateTime fromDate;
            DateTime toDate;
            DateTime midDate;

            if (month < 4)
            {
                fromDate = new DateTime(year, 1, 1);
                midDate = new DateTime(year, 2, 15);
                toDate = new DateTime(year, 3, 31);
                quarter.Number = 1;
            }
            else if (month < 7)
            {
                fromDate = new DateTime(year, 4, 1);
                midDate = new DateTime(year, 5, 15);
                toDate = new DateTime(year, 6, 30);
                quarter.Number = 2;
            }
            else if (month < 10)
            {
                fromDate = new DateTime(year, 7, 1);
                midDate = new DateTime(year, 8, 15);
                toDate = new DateTime(year, 9, 30);
                quarter.Number = 3;
            }
            else
            {
                fromDate = new DateTime(year, 10, 1);
                midDate = new DateTime(year, 11, 15);
                toDate = new DateTime(year, 12, 31);
                quarter.Number = 4;
            }

            quarter.From = fromDate;
            quarter.To = toDate;
            quarter.MidDate = midDate;

            return quarter;
        }

        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken securityToken)
        {
            var rawWeeks = _timeRepository.GetWeeksInQuarter(quarter, securityToken).ToList();
            _holidayService.AddHolidayHours(quarter, rawWeeks);
            rawWeeks.FirstOrDefault()?.AddQuarterEdgeHours(quarter);

            if (rawWeeks.Count > 1)
                rawWeeks.LastOrDefault()?.AddQuarterEdgeHours(quarter);

            rawWeeks.ForEach(week => week.CategorizeHours());
            return rawWeeks;
        }

        public IEnumerable<Week> GetNormalizedWeeks(List<Week> weeksList)
        {
            var weeks = weeksList.ConvertAll(w => w.Clone());

            var lowWeeks = weeks.Where(w => w.WorkHours + w.HolidayHours + w.QuarterEdgeHours < ApplicationConstants.NormalWorkWeek);
            var moveableWeeks = weeks.Where(w => w.MoveableOvertimeHours > 0).ToList();
            MoveHours(lowWeeks, moveableWeeks, "MoveableOvertimeHours");
            
            lowWeeks = weeks.Where(w => w.WorkHours + w.HolidayHours + w.QuarterEdgeHours < ApplicationConstants.NormalWorkWeek);
            var interestWeeks = weeks.Where(w => w.InterestHours > 0).ToList();
            MoveHours(lowWeeks, interestWeeks, "InterestHours");
            
            return weeks;
        }

        public IEnumerable<Month> GetNormalizedMonths(List<Month> monthsList)
        {
            var months = monthsList.ConvertAll(m => m.Clone());

            List<Month> lowMonths = months.Where(m => m.Hours < ApplicationConstants.AwesomeThursdayApproximation).ToList();
            List<Month> highMonths = months.Where(m => m.Hours > ApplicationConstants.AwesomeThursdayApproximation).ToList();
            
            MoveHours2(lowMonths, highMonths, null);
            
            return months;
        }

        private static void MoveHours<T>(IEnumerable<IAbsorbable<T>> lowHoursElements, List<T> moveableHoursElements, string propertyName)
        {
            foreach (var lowHoursElement in lowHoursElements)
            {
                var elementsAbsorbed = 0;
                foreach (var moveableHoursElement in moveableHoursElements)
                {
                    var doneAbsorbing = lowHoursElement.AbsorbHours(moveableHoursElement, propertyName);
                    if (doneAbsorbing)
                        break;
                    elementsAbsorbed++;
                }
                moveableHoursElements.RemoveRange(0, elementsAbsorbed);
            }
        }
        
        private static void MoveHours2(List<Month> lowHoursElements, List<Month> moveableHoursElements, string propertyName)
        {
            foreach (var lowHoursElement in lowHoursElements)
            {
                var elementsAbsorbed = 0;
                foreach (var moveableHoursElement in moveableHoursElements)
                {
                    var doneAbsorbing = lowHoursElement.AbsorbHours(moveableHoursElement, propertyName);
                    if (doneAbsorbing)
                        break;
                    elementsAbsorbed++;
                }
                moveableHoursElements.RemoveRange(0, elementsAbsorbed);
            }
        }
    }
}
