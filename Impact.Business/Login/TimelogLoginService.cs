using TimeLog.TransactionalApi.SDK;
using ExecutionStatus = TimeLog.TransactionalApi.SDK.OrganisationService.ExecutionStatus;
using SecurityToken = TimeLog.TransactionalApi.SDK.ProjectManagementService.SecurityToken;

namespace Impact.Business.Login
{
    public class TimelogLoginService : ILoginService
    {
        public bool IsAuthorized(string username, string password, out SecurityToken securityToken, out string fullName)
        {
            var authorized = SecurityHandler.Instance.TryAuthenticate(username, password, out var messages);
            securityToken = authorized ? ProjectManagementHandler.Instance.Token : null;
            fullName = authorized ? GetFullName() : string.Empty;

            return authorized;
        }

        private static string GetFullName()
        {
            var token = OrganisationHandler.Instance.Token;
            var response = OrganisationHandler.Instance.OrganisationClient.GetEmployeeByUsername(token.Initials, token);
            return response.ResponseState == ExecutionStatus.Success
                ? response.Return[0].Fullname
                : string.Empty;
        }

        public string FailedLoginMessageHtml()
        {
            return "<strong>Login Failed</strong><br/>Looks like your login failed. Did you just use your AD login?<br/>This app uses data from the Timelog API, therefore it needs your <strong>Timelog Login</strong>";
        }
    }
}