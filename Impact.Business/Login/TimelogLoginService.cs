﻿using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Impact.Core.Constants;
using Impact.Core.Model;
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

            var employee = GetTransactionalEmployee(username);
            if (employee == null)
                throw new NullReferenceException("GetEmployee failed. This should NEVER happen. How can you even be logged in if you do not exists in timelog. It makes no sense\n" +
                                                 "Please screenshot this error page and send it to PBM");

            var reportingEmployee = GetReportingEmployee(username);

            profile.FirstName = employee.FirstName;
            profile.LastName = employee.LastName;
            profile.FullName = employee.Fullname;
            profile.Initials = employee.Initials;
            profile.EmployeeId = employee.EmployeeID;
            profile.Title = employee.Title;
            profile.Department = employee.DepartmentName;
            profile.DepartmentId = reportingEmployee.DepartmentId;
            profile.CostPrice = employee.CostPrice;
            profile.HourlyRate = employee.HourlyRate;
            profile.HiredDate = employee.HiredDate;
            profile.IsDeveloper = IsDeveloper(employee);

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

        private static Profile GetReportingEmployee(string initials)
        {
            XmlNode employeeRaw = ReportingClient.GetEmployeesRaw(
                ReportingHandler.SiteCode,
                ReportingHandler.ApiId,
                ReportingHandler.ApiPassword,
                TimeLog.ReportingApi.SDK.Employee.All,
                initials,
                TimeLog.ReportingApi.SDK.Department.All,
                TimeLog.ReportingApi.SDK.EmployeeStatus.Active).FirstChild;

            var xnsm = new XmlNamespaceManager(employeeRaw.OwnerDocument.NameTable);
            xnsm.AddNamespace(employeeRaw.Prefix, employeeRaw.NamespaceURI);
            
            return new Profile
            {
                EmployeeId = int.Parse(employeeRaw.Attributes?["ID"].Value ?? "-1"),
                FirstName = employeeRaw.SelectSingleNode("tlp:FirstName", xnsm)?.InnerText ?? "",
                LastName = employeeRaw.SelectSingleNode("tlp:LastName", xnsm)?.InnerText ?? "",
                FullName = employeeRaw.SelectSingleNode("tlp:FullName", xnsm)?.InnerText ?? "",
                Initials = employeeRaw.SelectSingleNode("tlp:Initials", xnsm)?.InnerText ?? "",
                Title = employeeRaw.SelectSingleNode("tlp:Title", xnsm)?.InnerText ?? "",
                Email = employeeRaw.SelectSingleNode("tlp:Email", xnsm)?.InnerText ?? "",
                Department = employeeRaw.SelectSingleNode("tlp:DepartmentName", xnsm)?.InnerText ?? "",
                DepartmentId = int.Parse(employeeRaw.SelectSingleNode("tlp:DepartmentNameID", xnsm)?.InnerText ?? "-1"),
                HiredDate = DateTime.Parse(employeeRaw.SelectSingleNode("tlp:HiredDate", xnsm)?.InnerText),
                CostPrice = double.Parse(employeeRaw.SelectSingleNode("tlp:CostPrice", xnsm)?.InnerText ?? "0", CultureInfo.InvariantCulture)
            };
        }

        private static bool IsDeveloper(Employee employee)
        {
            if (!employee.IsHired)
                return false;

            var containsDeveloper = employee.Title.IndexOf("developer", StringComparison.InvariantCultureIgnoreCase) >= 0;
            var unitDeveloper = UnitDeveloper.IsMatch(employee.DepartmentName);

            return containsDeveloper || unitDeveloper;
        }

        public string FailedLoginMessageHtml()
        {
            return "<strong>Login Failed</strong><br/>Looks like your login failed. Did you just use your AD login?<br/>This app uses data from the Timelog API, therefore it needs your <strong>Timelog Login</strong>";
        }
    }
}