using System.Collections.Generic;
using System.Xml;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public interface IAddRegistrationStrategy<out T>
    {
        void AddRegistration(WorkUnitFlat registration);
        IEnumerable<T> GetList();
    }
}