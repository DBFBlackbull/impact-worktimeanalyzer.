using System;
using System.Globalization;

namespace Impact.Core.Model
{
    public class VacationDay
    {
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public double VacationHours { get; set; }
        public double ExtraVacationHours { get; set; }

        public VacationDay(DateTime date)
        {
            Date = date;
        }
        
        public VacationDay(DateTime date, string name) : this(date)
        {
            Name = name;
        }

        public string GetHours()
        {
            decimal result;
            
            if (VacationHours > 0 && ExtraVacationHours > 0)
                result = Math.Round(Convert.ToDecimal(VacationHours + ExtraVacationHours), 2);
            else if (VacationHours > 0)
                result = Math.Round(Convert.ToDecimal(VacationHours), 2);
            else
                result = Math.Round(Convert.ToDecimal(ExtraVacationHours), 2) * -1;

            return result.ToString(CultureInfo.InvariantCulture);
        }
    }
}