using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Impact.Core.Constants;
using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.OrganisationService;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.OrganisationService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.Business.Login
{
    public class TimelogLoginService : ILoginService
    {
        private static readonly OrganisationServiceClient Client = OrganisationHandler.Instance.OrganisationClient;
        private const int PageSize = 100;
        private static readonly Regex UnitDeveloper = new Regex(@"^Unit\s\d\s-\s[BF]E", RegexOptions.Compiled);

        public bool IsAuthorized(string username, string password, out SecurityToken securityToken, out string fullName, out bool isDeveloper, out DateTime hireDate)
        {
            fullName = string.Empty;
            isDeveloper = false;
            securityToken = null;
            hireDate = DateTime.MinValue;
            if (!SecurityHandler.Instance.TryAuthenticate(username, password, out var messages))
                return false;

            var employee = GetEmployee(username);
            if (employee == null)
                throw new NullReferenceException("GetEmployee failed. This should NEVER happen. How can you even be logged in if you do not exists in timelog. It makes no sense\n" +
                                                 "Please screenshot this error page and send it to PBM");
            
            fullName = employee.Fullname;
            isDeveloper = IsDeveloper(employee);
            hireDate = employee.HiredDate; 
            securityToken = ProjectManagementHandler.Instance.Token;

            return true;
        }

        private static Employee GetEmployee(string username)
        {
            var token = OrganisationHandler.Instance.Token;
            
            var pageIndex = 1;
            Employee[] employees;

            do
            {
                var response = Client.GetEmployeesPaged(pageIndex, PageSize, token);
                if (response.ResponseState != ExecutionStatus.Success)
                    return null;

                employees = response.Return;
                var employee = employees.FirstOrDefault(e => e.Initials == username.ToUpper());
                if (employee != null)
                    return employee;

                pageIndex++;
            } while (employees.Length == PageSize);

            return null;
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