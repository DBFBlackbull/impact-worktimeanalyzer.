using TimeLog.TransactionalApi.SDK;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Login
{
    public class TimelogLoginService : ILoginService
    {
        public bool IsAuthorized(string username, string password, out SecurityToken securityToken)
        {
            var authorized = SecurityHandler.Instance.TryAuthenticate(username, password, out var messages);
            securityToken = authorized ? ProjectManagementHandler.Instance.Token : null;
            return authorized;
        }

        public string FailedLoginMessageHtml()
        {
            return "<strong>Login Failed</strong><br/>Looks like your login failed. Did you just use your AD login?<br/>This app uses data from the Timelog API, therefore it needs your <strong>Timelog Login</strong>";
        }
    }
}