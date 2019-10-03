﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Strategies;
using TimeLog.ReportingApi.SDK;
using TimeLog.ReportingApi.SDK.ReportingService;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        private static readonly ProjectManagementServiceClient TransactionalClient = ProjectManagementHandler.Instance.ProjectManagementClient;
        private static readonly ServiceSoapClient ReportingClient = ServiceHandler.Instance.Client;

        private const int PageSize = 500;

        public IEnumerable<Week> GetRawWeeksInQuarter(Quarter quarter, Profile profile, SecurityToken token)
        {
            var from = quarter.From;
            while (from.DayOfWeek != DayOfWeek.Monday)
            {
                from = from.AddDays(-1);
            }
            var to = quarter.To;
            while (to.DayOfWeek != DayOfWeek.Friday)
            {
                to = to.AddDays(1);
            }
            
            var workingHours = GetWorkingHours(from, to, profile);
            var weeks = GetWorkUnitsData<Week>(quarter.From, quarter.To, token, new AddWeekStrategy(workingHours)).ToList();

            return weeks;
        }

        public IEnumerable<Month> GetAwesomeThursdays(DateTime hiredDate, SecurityToken token)
        {
            return GetWorkUnitsData<Month>(hiredDate, DateTime.Now, token, new AddMonthStrategy(hiredDate));
        }

        public IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, SecurityToken token, Profile profile)
        {
            var workingHours = GetWorkingHours(from, to, profile);
            var vacationDays = GetVacationRegistrations(from, to, profile, new AddVacationDayStrategy(workingHours));
//            var workUnitsData = GetWorkUnitsData<VacationDay>(@from, to, token, new AddVacationDayStrategy()).ToList();
            return vacationDays;
        }

        private static IEnumerable<T> GetWorkUnitsData<T>(DateTime from, DateTime to, SecurityToken token, IAddRegistrationStrategy<T> strategy)
        {
            WorkUnitFlat[] registrations;
            var pageIndex = 1;
            do
            {
                var result = TransactionalClient.GetWorkPaged(token.Initials, from, to, pageIndex, PageSize, token);
                if (result.ResponseState != ExecutionStatus.Success)
                    break;

                registrations = result.Return;

                foreach (var registration in registrations)
                    strategy.AddRegistration(registration);
                
                pageIndex++;
            } while (registrations.Length == PageSize);

            return strategy.GetList();
        }

        private static IEnumerable<VacationDay> GetVacationRegistrations(DateTime from, DateTime to, Profile profile, AddVacationDayStrategy strategy)
        {
            var timeOffRegistrationsRaw = ReportingClient.GetTimeOffRegistrationsRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                profile.EmployeeId,
                profile.DepartmentId,
                from,
                to);

            strategy.AddNamespace(timeOffRegistrationsRaw);
            
            foreach (XmlNode registration in timeOffRegistrationsRaw.ChildNodes)
                strategy.AddRegistration(registration);
            
            return strategy.GetList();
        }

        private static Dictionary<DateTime, decimal> GetWorkingHours(DateTime from, DateTime to, Profile profile)
        {
            var workingHours = ReportingClient.GetWorkingHoursRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                profile.EmployeeId,
                profile.DepartmentId,
                EmployeeStatus.Active,
                GetReportingDateString(@from),
                GetReportingDateString(to));

            var xnsm = new XmlNamespaceManager(workingHours.OwnerDocument.NameTable);
            xnsm.AddNamespace(workingHours.Prefix, workingHours.NamespaceURI);
            
            var dictionary = new Dictionary<DateTime, decimal>();
            foreach (XmlNode workingHour in workingHours)
            {
                var workHoursString = workingHour.SelectSingleNode("tlp:Hours", xnsm)?.InnerText;
                var isParsed = TryGetReportingDecimal(workHoursString, out var workHours);
                if (!isParsed)
                    workHours = profile.NormalWorkDay;
                
                var date = GetReportingDateTime(workingHour.SelectSingleNode("tlp:Date", xnsm)?.InnerText);
                dictionary.Add(date, workHours);
            }

            var zeroDays = dictionary
                .Where(kv => kv.Value == 0)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Saturday)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Sunday)
                .ToList();
            foreach (var kvp in zeroDays)
            {
                var lookupDate = kvp.Key;
                
                var workHours = dictionary[kvp.Key];
                while (workHours == 0)
                {
                    if (lookupDate.Day != 1)
                    {
                        lookupDate = lookupDate.AddDays(-1);
                        workHours = dictionary[lookupDate];
                    }
                    else
                    {
                        while (workHours == 0)
                        {
                            lookupDate = lookupDate.AddDays(1);
                            workHours = dictionary[lookupDate];
                        }
                    }
                }
                
                dictionary[kvp.Key] = workHours;
            }

            return dictionary;
        }

        public static string GetReportingDateString(DateTime dateTime)
        {
            return dateTime.ToString("s");
        }

        public static DateTime GetReportingDateTime(string dateString)
        {
            return DateTime.Parse(dateString);
        }

        public static bool TryGetReportingDecimal(string decimalString, out decimal reportingDecimal)
        {
            var tryParse = decimal.TryParse(decimalString,
                NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint,
                ApplicationConstants.EnglishCultureInfo.NumberFormat, out reportingDecimal);
            reportingDecimal = reportingDecimal.Normalize();
            return tryParse;
        }
    }
}
