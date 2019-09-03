using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddVacationDayStrategy : IAddRegistrationStrategy<VacationDay>
    {
        private const string VacationId = "20"; 
        private const string ExtraVacationId = "60";
        private Dictionary<DateTime, VacationDay> VacationDays { get; }

        public AddVacationDayStrategy()
        {
            VacationDays = new Dictionary<DateTime, VacationDay>();
        }

        public void AddRegistration(WorkUnitFlat registration)
        {
            if (registration.SalaryCode != VacationId && registration.SalaryCode != ExtraVacationId)
                return;
            
            var date = registration.Date;
            if (!VacationDays.TryGetValue(date, out var vacationDay))
                VacationDays[date] = vacationDay = new VacationDay(date);

            switch (registration.SalaryCode)
            {
                case VacationId:
                    vacationDay.VacationHours = registration.Hours;
                    break;
                case ExtraVacationId:
                    vacationDay.ExtraVacationHours = registration.Hours;
                    break;
            }
        }

        public IEnumerable<VacationDay> GetList()
        {
            return VacationDays.Values.OrderBy(v => v.Date).ToList();
        }
    }
}