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
            return $"Ferieår {StartDate.Year}/{EndDate.Year}";
        }

        public string GetLongDisplayString()
        {
            // Special case for mini-vacation. Remove in 5 years, when no longer relevant
            if (StartDate.Year == 2020 && EndDate.Year == 2020)
                return ApplicationConstants.DanishCultureInfo.TextInfo
                    .ToTitleCase($"Mini-ferieår: {StartDate:d MMMM yyyy} - {EndDate:d MMMM yyyy}");
            
            return ApplicationConstants.DanishCultureInfo.TextInfo
                .ToTitleCase($"Ferieår: {StartDate:d MMMM yyyy} - {EndDate:d MMMM yyyy}");
        }
    }
}