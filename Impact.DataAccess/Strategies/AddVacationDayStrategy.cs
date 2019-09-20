using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Impact.Core.Constants;
using Impact.Core.Model;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.DataAccess.Strategies
{
    public class AddVacationDayStrategy : IAddRegistrationStrategy<VacationDay>
    {
        private const string VacationId = "20"; 
        private const string ExtraVacationId = "60";
        private XmlNamespaceManager _xnsm;
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
                    return;
                case ExtraVacationId:
                    vacationDay.ExtraVacationHours = registration.Hours;
                    return;
            }
        }

        public IEnumerable<VacationDay> GetList()
        {
            return VacationDays.Values.OrderBy(v => v.Date).ToList();
        }

        public void AddNamespace(XmlNode node)
        {
            _xnsm = new XmlNamespaceManager(node.OwnerDocument.NameTable);
            _xnsm.AddNamespace(node.Prefix, node.NamespaceURI);
        }
        
        public void AddRegistration(XmlNode registration)
        {
            var timeOffCode = registration.SelectSingleNode("tlp:TimeOffCode", _xnsm)?.InnerText;
            if (timeOffCode != VacationId && timeOffCode != ExtraVacationId)
                return;
            
            var date = DateTime.Parse(registration.SelectSingleNode("tlp:Date", _xnsm)?.InnerText);
            var hours = double.Parse(registration.SelectSingleNode("tlp:RegHours", _xnsm)?.InnerText ?? "0", CultureInfo.InvariantCulture);
            var note = registration.SelectSingleNode("tlp:Note", _xnsm)?.InnerText ?? "";

            if (!VacationDays.TryGetValue(date, out var vacationDay))
                VacationDays[date] = vacationDay = new VacationDay(date, note);
            
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