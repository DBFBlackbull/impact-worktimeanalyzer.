using System;
using Impact.Core.Constants;

namespace Impact.Core.Model
{
    public class VacationYear
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalVacationDays { get; set; }
        public decimal TotalExtraVacationDays { get; set; }
        public VacationYear(DateTime start, DateTime end)
        {
            StartDate = start;
            EndDate = end;
            TotalVacationDays = 25;
            TotalExtraVacationDays = 5;
        }

        public string GetShortDisplayString()
        {
            // Special case for mini-vacation. Remove in 5 years, when no longer relevant
            if (StartDate == ApplicationConstants.MiniVacationStart && EndDate == ApplicationConstants.MiniVacationEnd)
                return $"Mini-ferieår {StartDate.Year}/{EndDate.Year}";
            
            return $"Ferieår {StartDate.Year}/{EndDate.Year}";
        }

        public string GetLongDisplayString()
        {
            var displayStartDate = StartDate.ToString("d MMMM yyyy", ApplicationConstants.DanishCultureInfo.DateTimeFormat);
            var displayEndDate = EndDate.ToString("d MMMM yyyy", ApplicationConstants.DanishCultureInfo.DateTimeFormat);
            // Special case for mini-vacation. Remove in 5 years, when no longer relevant
            if (StartDate == ApplicationConstants.MiniVacationStart && EndDate == ApplicationConstants.MiniVacationEnd)
                return ApplicationConstants.DanishCultureInfo.TextInfo.ToTitleCase($"Mini-ferieår: {displayStartDate} - {displayEndDate}");

            return ApplicationConstants.DanishCultureInfo.TextInfo.ToTitleCase($"Ferieår: {displayStartDate} - {displayEndDate}");
        }
    }
}