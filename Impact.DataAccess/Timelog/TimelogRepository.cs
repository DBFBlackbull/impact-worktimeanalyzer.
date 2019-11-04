using System;
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
using Allocation = TimeLog.ReportingApi.SDK.Allocation;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.ProjectManagementService.ExecutionStatus;
using Project = Impact.Core.Model.Project;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;
using Task = Impact.Core.Model.Task;
using WorkUnit = TimeLog.ReportingApi.SDK.WorkUnit;

namespace Impact.DataAccess.Timelog
{
    public class TimeLogRepository : ITimeRepository
    {
        private static readonly ProjectManagementServiceClient TransactionalClient = ProjectManagementHandler.Instance.ProjectManagementClient;
        private static readonly ServiceSoapClient ReportingClient = ServiceHandler.Instance.Client;
        private static readonly Dictionary<string, Dictionary<DateTime, decimal>> WorkingHours = new Dictionary<string, Dictionary<DateTime, decimal>>();

        private const int PageSize = 500;

        public IEnumerable<Week> GetRawWeeksInQuarter(Quarter quarter, Profile profile, SecurityToken token)
        {
            var from = quarter.From;
            while (from.DayOfWeek != DayOfWeek.Monday && 
                   from.DayOfWeek != DayOfWeek.Saturday && 
                   from.DayOfWeek != DayOfWeek.Sunday)
            {
                from = from.AddDays(-1);
            }
            var to = quarter.To;
            while (to.DayOfWeek != DayOfWeek.Friday &&
                   to.DayOfWeek != DayOfWeek.Saturday && 
                   to.DayOfWeek != DayOfWeek.Sunday)
            {
                to = to.AddDays(1);
            }

            ValidateWorkingHours(from, to, profile);
            var weeks = GetWorkUnitsData<Week>(quarter.From, quarter.To, token, new AddWeekStrategy(WorkingHours[profile.Initials])).ToList();

            return weeks;
        }

        public IEnumerable<Month> GetAwesomeThursdays(DateTime hiredDate, SecurityToken token)
        {
            return GetWorkUnitsData<Month>(hiredDate, DateTime.Now, token, new AddMonthStrategy(hiredDate));
        }

        public IEnumerable<VacationDay> GetVacationDays(DateTime from, DateTime to, Profile profile, SecurityToken token)
        {
            ValidateWorkingHours(from, to, profile);
            var vacationDays = GetVacationRegistrations(from, to, profile, new AddVacationDayStrategy(WorkingHours[profile.Initials]));
//            var workUnitsData = GetWorkUnitsData<VacationDay>(@from, to, token, new AddVacationDayStrategy()).ToList();
            return vacationDays;
        }

        public IEnumerable<TimeRegistration> GetRegistrationsWithJiraId(string jiraId, DateTime from, DateTime to, Profile profile, SecurityToken token)
        {
            var workUnitsRaw = ReportingClient.GetWorkUnitsRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                WorkUnit.All,
                profile.EmployeeId,
                Allocation.All,
                TimeLog.ReportingApi.SDK.Task.All,
                TimeLog.ReportingApi.SDK.Project.All,
                profile.DepartmentId,
                GetReportingDateString(from),
                GetReportingDateString(to)
            );

            var xnsm = new XmlNamespaceManager(workUnitsRaw.OwnerDocument.NameTable);
            xnsm.AddNamespace(workUnitsRaw.Prefix, workUnitsRaw.NamespaceURI);

            var xmlNodeList = workUnitsRaw.SelectNodes($"//tlp:WorkUnit[tlp:AdditionalTextField='{jiraId}']", xnsm);
            var registrationsWithJiraId = new List<TimeRegistration>();
            if (xmlNodeList == null)
                return registrationsWithJiraId;

            foreach (XmlNode xmlNode in xmlNodeList)
            {
                var workUnit = new WorkUnit(xmlNode, xnsm);
                var timeRegistration = new TimeRegistration
                (
                    workUnit.AdditionalTextField,
                    workUnit.TaskName,
                    workUnit.ProjectName,
                    workUnit.Date,
                    workUnit.Note,
                    Convert.ToDecimal(workUnit.RegHours).Normalize()
                );
                registrationsWithJiraId.Add(timeRegistration);
            }

            registrationsWithJiraId = registrationsWithJiraId.OrderBy(r => r.Date).ToList();

            return registrationsWithJiraId;
        }

        public IEnumerable<Project> GetTasks(string initials, SecurityToken token)
        {
            var projects = new Dictionary<int, Project>();

            var timelogTasks = TransactionalClient.GetTasksAllocatedToEmployee(initials, token).Return;
            foreach (var timelogTask in timelogTasks)
            {
                var projectId = timelogTask.Details.ProjectHeader.ID;
                var projectName = timelogTask.Details.ProjectHeader.Name;
                if (!projects.TryGetValue(projectId, out var project))
                    projects[projectId] = project = new Project(projectId, projectName);
                
                project.Tasks.Add(new Task(timelogTask.TaskID, timelogTask.Name));
            }

            return projects.Values.ToList();
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

        private static void ValidateWorkingHours(DateTime from, DateTime to, Profile profile)
        {
            if (!WorkingHours.TryGetValue(profile.Initials, out var dictionary))
                WorkingHours[profile.Initials] = dictionary = new Dictionary<DateTime, decimal>();
            
            var current = from;
            var neededDates = new SortedSet<DateTime>();
            while (current <= to)
            {
                neededDates.Add(current);
                current = current.AddDays(1);
            }

            neededDates.ExceptWith(dictionary.Keys);
            var valid = !neededDates.Any();
            if (valid)
                return;

            UpdateWorkingHoursDictionary(neededDates, profile);
        }

        private static void UpdateWorkingHoursDictionary(ISet<DateTime> neededDates, Profile profile)
        {
            var start = neededDates.First();
            var end = neededDates.Last();

            var dictionary = GetWorkingHoursDictionary(start, end, profile);
            while (!dictionary.Any())
            {
                var timeSpan = end - start;
                start = start.Add(timeSpan);
                end = end.Add(timeSpan);
                dictionary = GetWorkingHoursDictionary(start, end, profile);
            }
            
            var missingDates = neededDates.Except(dictionary.Keys).ToDictionary(kv => kv, kv => 0m);
            dictionary = dictionary.Union(missingDates).ToDictionary(kv => kv.Key, kv => kv.Value);

            HandleZeroDays(dictionary);
            foreach (var neededDate in neededDates)
            {
                WorkingHours[profile.Initials][neededDate] = dictionary[neededDate];
            }
        }

        private static IDictionary<DateTime, decimal> GetWorkingHoursDictionary(DateTime start, DateTime end, Profile profile)
        {
            var workingHours = ReportingClient.GetWorkingHoursRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                profile.EmployeeId,
                profile.DepartmentId,
                EmployeeStatus.All,
                GetReportingDateString(start),
                GetReportingDateString(end));

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

            return dictionary;
        }

        private static void HandleZeroDays(IDictionary<DateTime, decimal> dictionary)
        {
            var zeroDays = dictionary
                .Where(kv => kv.Value == 0)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Saturday)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Sunday)
                .ToList();
            
            foreach (var kvp in zeroDays)
            {
                var lookupDate = kvp.Key;

                var successLookup = lookupDate.Day > 1;
                var workHours = dictionary[kvp.Key];
                while (workHours == 0 && successLookup)
                {
                    lookupDate = lookupDate.AddDays(-1);
                    successLookup = dictionary.TryGetValue(lookupDate, out workHours) 
                                    && lookupDate.Day < 1;
                }

                lookupDate = kvp.Key;
                while (workHours == 0)
                {
                    lookupDate = lookupDate.AddDays(1);
                    dictionary.TryGetValue(lookupDate, out workHours);
                }
                
                dictionary[kvp.Key] = workHours;
            }
        }

        private static string GetReportingDateString(DateTime dateTime)
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
