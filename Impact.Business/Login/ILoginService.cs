using System;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Business.Login
{
    public interface ILoginService
    {
        bool IsAuthorized(string username, string password, out SecurityToken securityToken, out string fullName, out bool isDeveloper, out DateTime hireDate);
        string FailedLoginMessageHtml();
    }
}