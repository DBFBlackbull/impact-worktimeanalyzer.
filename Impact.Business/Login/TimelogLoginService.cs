﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using TimeLog.ReportingApi.SDK;
using TimeLog.ReportingApi.SDK.ReportingService;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.OrganisationService;
using Employee = TimeLog.TransactionalApi.SDK.OrganisationService.Employee;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.OrganisationService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.Business.Login
{
    public class TimelogLoginService : ILoginService
    {
        private static readonly OrganisationServiceClient TransactionalClient = OrganisationHandler.Instance.OrganisationClient;
        private static readonly ServiceHandler ReportingHandler = ServiceHandler.Instance;
        private static readonly ServiceSoapClient ReportingClient = ReportingHandler.Client;
        private const int PageSize = 100;
        private static readonly Regex UnitDeveloper = new Regex(@"^Unit\s\d\s-\s[BF]E", RegexOptions.Compiled);

        public bool IsAuthorized(string username, string password, out SecurityToken securityToken, out Profile profile)
        {
            profile = new Profile();
            securityToken = null;
            if (!SecurityHandler.Instance.TryAuthenticate(username, password, out var messages))
                return false;

            profile = GetReportingProfile(username);
            if (profile == null)
                throw new NullReferenceException("GetEmployee failed. This should NEVER happen. How can you even be logged in if you do not exists in timelog. It makes no sense\n" +
                                                 "Please screenshot this error page and send it to PBM");

            profile.IsDeveloper = IsDeveloper(profile.Title, profile.DepartmentName);
            profile.NormalWorkDay = GetReportingNormalWorkDay(profile.EmployeeId, profile.DepartmentId);
            profile.NormalWorkWeek = (profile.NormalWorkDay * 5).Normalize();
            profile.NormalWorkMonth = Math.Round(profile.NormalWorkWeek * 52m / 12m, 2, MidpointRounding.AwayFromZero).Normalize(); 

            securityToken = ProjectManagementHandler.Instance.Token;

            return true;
        }

        private static Employee GetTransactionalEmployee(string initials)
        {
            var token = OrganisationHandler.Instance.Token;
            var pageIndex = 1;
            Employee[] employees;

            do
            {
                var response = TransactionalClient.GetEmployeesPaged(pageIndex, PageSize, token);
                if (response.ResponseState != ExecutionStatus.Success)
                    return null;

                employees = response.Return;
                var employee = employees.FirstOrDefault(e => e.Initials == initials.ToUpper());
                if (employee != null)
                    return employee;

                pageIndex++;
            } while (employees.Length == PageSize);

            return null;
        }

        private static Profile GetReportingProfile(string initials, int employeeId = 0)
        {
            XmlNode employeeRaw = ReportingClient.GetEmployeesRaw(
                ReportingHandler.SiteCode,
                ReportingHandler.ApiId,
                ReportingHandler.ApiPassword,
                employeeId,
                initials,
                TimeLog.ReportingApi.SDK.Department.All,
                TimeLog.ReportingApi.SDK.EmployeeStatus.Active).FirstChild;

            var xnsm = new XmlNamespaceManager(employeeRaw.OwnerDocument.NameTable);
            xnsm.AddNamespace(employeeRaw.Prefix, employeeRaw.NamespaceURI);

            var reportingProfile = new Profile
            {
                EmployeeId = int.Parse(employeeRaw.Attributes?["ID"].Value ?? "-1"),
                FirstName = employeeRaw.SelectSingleNode("tlp:FirstName", xnsm)?.InnerText ?? "",
                LastName = employeeRaw.SelectSingleNode("tlp:LastName", xnsm)?.InnerText ?? "",
                FullName = employeeRaw.SelectSingleNode("tlp:FullName", xnsm)?.InnerText ?? "",
                Initials = employeeRaw.SelectSingleNode("tlp:Initials", xnsm)?.InnerText ?? "",
                Title = employeeRaw.SelectSingleNode("tlp:Title", xnsm)?.InnerText ?? "",
                Email = employeeRaw.SelectSingleNode("tlp:Email", xnsm)?.InnerText ?? "",
                DepartmentName = employeeRaw.SelectSingleNode("tlp:DepartmentName", xnsm)?.InnerText ?? "",
                DepartmentId = int.Parse(employeeRaw.SelectSingleNode("tlp:DepartmentNameID", xnsm)?.InnerText ?? "-1"),
                HiredDate = TimeLogRepository.GetReportingDateTime(employeeRaw.SelectSingleNode("tlp:HiredDate", xnsm)?.InnerText),
                CostPrice = double.Parse(employeeRaw.SelectSingleNode("tlp:CostPrice", xnsm)?.InnerText ?? "0", CultureInfo.InvariantCulture)
            };
            return reportingProfile;
        }

        private static decimal GetReportingNormalWorkDay(int employeeId, int departmentId)
        {
            var normalWorkingHoursRaw = ReportingClient.GetEmployeeNormalWorkingHoursRaw(
                ReportingHandler.SiteCode,
                ReportingHandler.ApiId,
                ReportingHandler.ApiPassword,
                employeeId,
                departmentId,
                TimeLog.ReportingApi.SDK.EmployeeStatus.Active);

            if (normalWorkingHoursRaw.OwnerDocument == null) 
                return ApplicationConstants.NormalWorkDay;
            
            var xnsm = new XmlNamespaceManager(normalWorkingHoursRaw.OwnerDocument.NameTable);
            xnsm.AddNamespace(normalWorkingHoursRaw.Prefix, normalWorkingHoursRaw.NamespaceURI);

            foreach (XmlNode workingHoursXml in normalWorkingHoursRaw.ChildNodes)
            {
                var workingHoursString = workingHoursXml.SelectSingleNode("tlp:WorkingHours", xnsm)?.InnerText;
                var isParsed = TimeLogRepository.TryGetReportingDecimal(workingHoursString, out var workingHours);
                if (isParsed && workingHours > 0)
                    return workingHours.Normalize();
            }

            return ApplicationConstants.NormalWorkDay;
        }

        private static bool IsDeveloper(string title, string departmentName)
        {
            var containsDeveloper = title.IndexOf("developer", StringComparison.InvariantCultureIgnoreCase) >= 0;
            var unitDeveloper = UnitDeveloper.IsMatch(departmentName);

            return containsDeveloper || unitDeveloper;
        }

        public string FailedLoginMessageHtml()
        {
            return "<strong>Login Failed</strong><br/>Looks like your login failed. Did you just use your AD login?<br/>This app uses data from the Timelog API, therefore it needs your <strong>Timelog Login</strong>";
        }
    }
}