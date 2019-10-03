using System;
using System.Globalization;
using Impact.Core.Extension;

namespace Impact.Core.Model
{
    public class VacationDay
    {
        public DateTime Date { get; }
        public string Comment { get; }
        public decimal NormalWorkDay { get; }
        public decimal VacationHours { get; set; }
        public decimal ExtraVacationHours { get; set; }

        public VacationDay(DateTime date, decimal normalWorkDay)
        {
            Date = date;
            NormalWorkDay = normalWorkDay;
        }
        
        public VacationDay(DateTime date, decimal normalWorkDay, string comment) : this(date, normalWorkDay)
        {
            Comment = comment;
        }

        public decimal GetDecimalHours()
        {
            decimal result;
            
            if (VacationHours > 0 && ExtraVacationHours > 0)
                result = Math.Round(Convert.ToDecimal(VacationHours + ExtraVacationHours), 2);
            else if (VacationHours > 0)
                result = Math.Round(Convert.ToDecimal(VacationHours), 2);
            else
                result = Math.Round(Convert.ToDecimal(ExtraVacationHours), 2) * -1;

            return result.Normalize();
        }
        
        public string GetStringHours()
        {
            return GetDecimalHours().ToString(CultureInfo.InvariantCulture);
        }
    }
}