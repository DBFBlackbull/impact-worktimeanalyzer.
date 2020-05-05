using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;
using Task = System.Threading.Tasks.Task;
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
            var vacationDays = GetVacationRegistrations(from, to, profile);
//            var workUnitsData = GetWorkUnitsData<VacationDay>(@from, to, token, new AddVacationDayStrategy()).ToList();
            return vacationDays;
        }

        // Refactor this method to be even more threaded
        public IEnumerable<TimeRegistration> GetRegistrationsWithJiraId(string jiraId, int projectId, DateTime from, DateTime to, Profile profile, SecurityToken token)
        {
            var registrationsWithJiraId = new ConcurrentBag<TimeRegistration>();

            var future = DateTime.Now.Date.AddDays(1);
            // Get empty document just to get XmlNamespaceManager
            var emptyXmlNode = ReportingClient.GetWorkUnitsRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                WorkUnit.All,
                profile.EmployeeId,
                Allocation.All,
                TimeLog.ReportingApi.SDK.Task.All,
                projectId,
                Department.All,
                GetReportingDateString(future),
                GetReportingDateString(future)
            );
            
            var ownerDocument = emptyXmlNode.OwnerDocument;
            if (ownerDocument == null)
                return registrationsWithJiraId.ToList();
            
            var xmlNamespaceManager = new XmlNamespaceManager(ownerDocument.NameTable);
            xmlNamespaceManager.AddNamespace(emptyXmlNode.Prefix, emptyXmlNode.NamespaceURI);

            var allChildren = string.IsNullOrEmpty(jiraId);
            
            var tasks = new List<Task>();
            
            var start = from;
            while (start < to)
            {
                var nextDate = start.AddMonths(3);
                if (to < nextDate)
                    nextDate = to;

                var workUnitsRawTask = ReportingClient.GetWorkUnitsRawAsync(
                    ServiceHandler.Instance.SiteCode,
                    ServiceHandler.Instance.ApiId,
                    ServiceHandler.Instance.ApiPassword,
                    WorkUnit.All,
                    profile.EmployeeId,
                    Allocation.All,
                    TimeLog.ReportingApi.SDK.Task.All,
                    projectId,
                    Department.All,
                    GetReportingDateString(start),
                    GetReportingDateString(nextDate)
                );

                var continueWith = workUnitsRawTask.ContinueWith(task =>
                {
                    var xmlNodeList = allChildren 
                        ? task.Result.ChildNodes 
                        : task.Result.SelectNodes($"//tlp:WorkUnit[tlp:AdditionalTextField='{jiraId}']", xmlNamespaceManager);

                    if (xmlNodeList == null)
                        return;
                    
                    foreach (XmlNode xmlNode in xmlNodeList)
                    {
                        var workUnit = new WorkUnit(xmlNode, xmlNamespaceManager);
                        var timeRegistration = new TimeRegistration
                        (
                            workUnit.AdditionalTextField,
                            workUnit.TaskName,
                            workUnit.ProjectName,
                            workUnit.CustomerID,
                            workUnit.CustomerName,
                            workUnit.Date,
                            workUnit.Note,
                            Convert.ToDecimal(workUnit.RegHours).Normalize()
                        );
                        registrationsWithJiraId.Add(timeRegistration);
                    }
                });
                tasks.Add(continueWith);
                start = nextDate.AddDays(1);
            }

            Task.WaitAll(tasks.ToArray());

            var timeRegistrations = registrationsWithJiraId.OrderBy(r => r.Date).ToList();

            return timeRegistrations;
        }

        public IDictionary<int, string> GetProjects(string initials, SecurityToken token)
        {
            var projects = new Dictionary<int, string>();

            var timelogTasks = TransactionalClient.GetTasksAllocatedToEmployee(initials, token).Return;
            foreach (var timelogTask in timelogTasks)
            {
                var projectId = timelogTask.Details.ProjectHeader.ID;
                var projectName = timelogTask.Details.ProjectHeader.Name;
                if (!projects.ContainsKey(projectId))
                    projects[projectId] = projectName;
            }

            return projects;
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

        private static IEnumerable<VacationDay> GetVacationRegistrations(DateTime from, DateTime to, Profile profile)
        {
            var timeOffRegistrationsRaw = ReportingClient.GetTimeOffRegistrationsRaw(
                ServiceHandler.Instance.SiteCode,
                ServiceHandler.Instance.ApiId,
                ServiceHandler.Instance.ApiPassword,
                profile.EmployeeId,
                Department.All,
                from,
                to);

            var ownerDocument = timeOffRegistrationsRaw.OwnerDocument;
            if (ownerDocument == null)
                return new List<VacationDay>();
            
            var xmlNamespaceManager = new XmlNamespaceManager(ownerDocument.NameTable);
            xmlNamespaceManager.AddNamespace(timeOffRegistrationsRaw.Prefix, timeOffRegistrationsRaw.NamespaceURI);
            
            var strategy = new AddVacationDayStrategy(WorkingHours[profile.Initials], xmlNamespaceManager);
            
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

            HandleZeroDays(dictionary, start, end);
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
                Department.All,
                EmployeeStatus.All,
                GetReportingDateString(start),
                GetReportingDateString(end));

            var ownerDocument = workingHours.OwnerDocument;
            if (ownerDocument == null)
                return new Dictionary<DateTime, decimal>();
            
            var xmlNamespaceManager = new XmlNamespaceManager(ownerDocument.NameTable);
            xmlNamespaceManager.AddNamespace(workingHours.Prefix, workingHours.NamespaceURI);

            var dictionary = new Dictionary<DateTime, decimal>();
            foreach (XmlNode workingHour in workingHours)
            {
                var workHoursString = workingHour.SelectSingleNode("tlp:Hours", xmlNamespaceManager)?.InnerText ?? "";
                var isParsed = TryGetReportingDecimal(workHoursString, out var workHours);
                if (!isParsed)
                    workHours = profile.NormalWorkDay;

                var dateString = workingHour.SelectSingleNode("tlp:Date", xmlNamespaceManager)?.InnerText ?? "";
                var date = GetReportingDateTime(dateString);
                dictionary.Add(date, workHours);
            }

            return dictionary;
        }

        private static void HandleZeroDays(IDictionary<DateTime, decimal> dictionary, DateTime start, DateTime end)
        {
            var zeroDays = dictionary
                .Where(kv => kv.Value == 0)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Saturday)
                .Where(kv => kv.Key.DayOfWeek != DayOfWeek.Sunday)
                .ToList();
            
            foreach (var kvp in zeroDays)
            {
                var workHours = dictionary[kvp.Key];

                var lookupBackwardsDate = kvp.Key;
                var successLookup = lookupBackwardsDate.Day > 1;
                // Look back through the current month to find usable work hours.
                while (workHours == 0 && successLookup)
                {
                    lookupBackwardsDate = lookupBackwardsDate.AddDays(-1);
                    successLookup = dictionary.TryGetValue(lookupBackwardsDate, out workHours) 
                                    && lookupBackwardsDate.Day > 1;
                }

                var lookForwardDate = kvp.Key;
                successLookup = lookForwardDate < end;
                // Look forward until end to find usable work hours 
                while (workHours == 0 && successLookup)
                {
                    lookForwardDate = lookForwardDate.AddDays(1);
                    successLookup = dictionary.TryGetValue(lookForwardDate, out workHours) 
                                    && lookForwardDate < end;
                }

                successLookup = lookupBackwardsDate > start;
                // Look backwards until the start to find usable work hours
                while (workHours == 0 && successLookup)
                {
                    lookupBackwardsDate = lookupBackwardsDate.AddDays(-1);
                    successLookup = dictionary.TryGetValue(lookupBackwardsDate, out workHours) 
                                    && lookupBackwardsDate > start;
                }
                
                // If we haven't found a time at this point we give up and let the value be 0.
                
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
