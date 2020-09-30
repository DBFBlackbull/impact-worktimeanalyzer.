using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddVacationDayStrategy : IAddRegistrationStrategy<VacationDay>
    {
        private readonly Dictionary<DateTime, decimal> _workingHours;
        private readonly XmlNamespaceManager _xmlNamespaceManager;
        private const string VacationId = "20";
        private const string NewVacationId = "15"; // Introduced on the 30th of September. TimelogName '15 - Ferie (funktionærer)' 
        private const string ExtraVacationId = "60";
        private Dictionary<DateTime, VacationDay> VacationDays { get; }

        public AddVacationDayStrategy(Dictionary<DateTime, decimal> workingHours, XmlNamespaceManager xmlNamespaceManager)
        {
            _workingHours = workingHours;
            _xmlNamespaceManager = xmlNamespaceManager; 
            VacationDays = new Dictionary<DateTime, VacationDay>();
        }

        public void AddRegistration(WorkUnitFlat registration)
        {
            if (registration.SalaryCode != VacationId && registration.SalaryCode != ExtraVacationId)
                return;
            
            var date = registration.Date;
            if (!VacationDays.TryGetValue(date, out var vacationDay))
                VacationDays[date] = vacationDay = new VacationDay(date, _workingHours[date]);

            switch (registration.SalaryCode)
            {
                case VacationId:
                    vacationDay.VacationHours = Convert.ToDecimal(registration.Hours);
                    return;
                case NewVacationId:
                    vacationDay.VacationHours = Convert.ToDecimal(registration.Hours);
                    return;
                case ExtraVacationId:
                    vacationDay.ExtraVacationHours = Convert.ToDecimal(registration.Hours);
                    return;
            }
        }

        public IEnumerable<VacationDay> GetList()
        {
            return VacationDays.Values.OrderBy(v => v.Date).ToList();
        }
        
        public void AddRegistration(XmlNode registration)
        {
            var timeOffCode = registration.SelectSingleNode("tlp:TimeOffCode", _xmlNamespaceManager)?.InnerText;
            if (timeOffCode != VacationId && timeOffCode != ExtraVacationId)
                return;
            
            var date = DateTime.Parse(registration.SelectSingleNode("tlp:Date", _xmlNamespaceManager)?.InnerText);
            var hours = decimal.Parse(registration.SelectSingleNode("tlp:RegHours", _xmlNamespaceManager)?.InnerText ?? "0", CultureInfo.InvariantCulture);
            var note = registration.SelectSingleNode("tlp:Note", _xmlNamespaceManager)?.InnerText ?? "";

            if (!VacationDays.TryGetValue(date, out var vacationDay))
                VacationDays[date] = vacationDay = new VacationDay(date, _workingHours[date], note);
            
            switch (timeOffCode)
            {
                case VacationId:
                    vacationDay.VacationHours = hours;
                    return;
                case ExtraVacationId:
                    vacationDay.ExtraVacationHours = hours;
                    return;
            }
        }
    }
}