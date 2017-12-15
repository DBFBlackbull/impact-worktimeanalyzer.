using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Model;
using Impact.Website.Models;

namespace Impact.Website.Providers
{
    public class WeeksViewModelProvider
    {
        public static WeeksViewModel CreateWeeksViewModel(Quarter quarter, IEnumerable<Week> weeksList, List<Week> normalizedWeeks, bool isNormalized)
        {
            var weeksViewModel = new WeeksViewModel();
            weeksViewModel.GraphTitle = GraphTitle(quarter);
            weeksViewModel.Json = GetJson(weeksList);
            weeksViewModel.IsNormalized = isNormalized;
            weeksViewModel.NormalizedJson = GetJson(normalizedWeeks);
            return weeksViewModel;
        }
        
        private static List<object[]> GetJson(IEnumerable<Week> weeks)
        {
            List<object[]> googleFormatedWeeks = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Uge nummer", Type = "string"},
                    new Column {Label = "HolidayHours", Type = "number"},
                    new Column {Label = "Work,Ferie,Sygdom osv.", Type = "number"},
                    new Column {Label = "Interessetid : 0% løn", Type = "number"},
                    new Column {Label = "39-44 : 100% løn", Type = "number"},
                    new Column {Label = "44+ : 150% løn", Type = "number"}
                }
            };
            googleFormatedWeeks.AddRange(weeks.Select(week => week.ToArray()));
            return googleFormatedWeeks;
        }
        
        private static string GraphTitle(Quarter quarter)
        {
            switch (quarter.Number)
            {
                case 1:
                    return quarter.GetDisplayText() + ": Januar til Marts";
                case 2:
                    return quarter.GetDisplayText() + ": April til Juni";
                case 3:
                    return quarter.GetDisplayText() + ": Juli til September";
                case 4:
                    return quarter.GetDisplayText() + ": Oktober til December";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + quarter);
            }
        }
    }
}