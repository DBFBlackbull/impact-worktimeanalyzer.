using System.Collections.Generic;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public interface IAddRegistrationStrategy<T>
    {
        void AddRegistration(WorkUnitFlat registration);
        IEnumerable<T> GetList();
    }
}