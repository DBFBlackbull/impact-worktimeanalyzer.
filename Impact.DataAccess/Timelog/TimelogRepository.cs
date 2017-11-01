using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using TimeLog.TransactionalApi.SDK.SalaryService;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        private const decimal WorkConst = 37.5m;
        private const decimal InterestConst = 1.5m;
        private const decimal MoveableConst = 5m;

        public IEnumerable<Week> GetWeeksInQuarter(Quarter quarter, SecurityToken token)
        {
            var weeksToHoursDictionary = new Dictionary<string, double>();
            var calendar = CultureInfo.InvariantCulture.Calendar;

//            {
//                ResponseOfWorkUnitFlat workedPage = ProjectManagementHandler.Instance.ProjectManagementClient.GetWorkPaged(token.Initials, quarter.From, quarter.To, 1, 500, token);
//                WorkUnitFlat[] resultReturn;
//                if (workedPage.ResponseState == ExecutionStatus.Success)
//                    resultReturn = workedPage.Return;
//
//                ResponseOfAllocation allocationsToEmployee = ProjectManagementHandler.Instance.ProjectManagementClient.GetAllocationsToEmployee(token.Initials, token);
//                Allocation[] allocations;
//                if (allocationsToEmployee.ResponseState == ExecutionStatus.Success)
//                    allocations = allocationsToEmployee.Return;
//
//                ResponseOfWorkUnit responseOfWorkUnit = ProjectManagementHandler.Instance.ProjectManagementClient.GetEmployeeWork(token.Initials, quarter.From, quarter.To, token);
//                WorkUnit[] workUnits;
//                if (responseOfWorkUnit.ResponseState == ExecutionStatus.Success)
//                    workUnits = responseOfWorkUnit.Return;
//
//                var saleryToken = SalaryHandler.Instance.Token;
//                var responseCalendar = SalaryHandler.Instance.SalaryClient.GetHolidayCalendars(saleryToken);
//                HolidayCalendar[] holidayCalendars;
//                if (responseCalendar.ResponseState == TimeLog.TransactionalApi.SDK.SalaryService.ExecutionStatus.Success)
//                    holidayCalendars = responseCalendar.Return;
//
//                var responseWeeks = SalaryHandler.Instance.SalaryClient.GetNormalWorkweeks(saleryToken);
//                NormalWorkweek[] responseWeeksReturn;
//                if (responseWeeks.ResponseState == TimeLog.TransactionalApi.SDK.SalaryService.ExecutionStatus.Success)
//                    responseWeeksReturn = responseWeeks.Return;
//
//                ProjectManagementHandler.Instance.ProjectManagementClient.GetProjectByExternalKey("Impact", , token);
//                int i = 1;
//            }

            ResponseOfWorkUnitFlat result = ProjectManagementHandler.Instance.ProjectManagementClient.GetWorkPaged(token.Initials, quarter.From, quarter.To, 1, 500, token);
            if (result.ResponseState == ExecutionStatus.Success)
            {
                var workUnitFlats = result.Return;
                foreach (var workUnitFlat in workUnitFlats)
                {
                    var dateTime = workUnitFlat.Date;
                    var weekNumber = calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);

                    if (!weeksToHoursDictionary.ContainsKey(weekNumber.ToString()))
                        weeksToHoursDictionary[weekNumber.ToString()] = 0;

                    weeksToHoursDictionary[weekNumber.ToString()] += workUnitFlat.Hours;
                }
            }

            var weeks = new List<Week>(15);

            foreach (var weekNumber in weeksToHoursDictionary.Keys)
            {
                double hours = weeksToHoursDictionary[weekNumber];
                var convert = Convert.ToDecimal(hours);
                var round = Math.Round(convert, 1);

                weeks.Add(CreateWeek(weekNumber, round));
            }

            return weeks;
        }

        // So ungly please find a better solution. Really really
        private static Week CreateWeek(string weekNumber, decimal hours)
        {
            var week = new Week("Uge " + weekNumber);

            if (hours >= WorkConst)
            {
                week.WorkHours = WorkConst;
                hours -= WorkConst;
            }
            else
            {
                week.WorkHours = hours;
                return week;
            }

            if (hours >= InterestConst)
            {
                week.InterestHours = InterestConst;
                hours -= InterestConst;
            }
            else
            {
                week.InterestHours = hours;
                return week;
            }

            if (hours >= MoveableConst)
            {
                week.MoveableOvertimeHours = MoveableConst;
                hours -= MoveableConst;
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
