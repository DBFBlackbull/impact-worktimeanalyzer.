using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Login
{
    public interface ILoginService
    {
        bool IsAuthorized(string username, string password, out SecurityToken securityToken);
        string FailedLoginMessageHtml();
    }
}