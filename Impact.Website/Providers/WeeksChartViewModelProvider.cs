using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Business.Time;
using Impact.Core.Model;
using Impact.Website.Models;
using Impact.Website.Models.Charts;

namespace Impact.Website.Providers
{
    public class WeeksChartViewModelProvider
    {
        public static OverviewChartViewModel CreateWeeksViewModel(Quarter quarter, List<Week> weeksList, List<Week> normalizedWeeks, bool isNormalized)
        {
            var weeksViewModel = new OverviewChartViewModel();
            weeksViewModel.DivId = "weeks_chart";
            weeksViewModel.GraphTitle = GraphTitle(quarter);
            weeksViewModel.IsNormalized = isNormalized;
            weeksViewModel.YMax = YAxisMax(weeksList);
            weeksViewModel.Json = GetJson(weeksList, null);
            weeksViewModel.NormalizedJson = GetJson(normalizedWeeks, 0);
            return weeksViewModel;
        }

        private static int YAxisMax(List<Week> weeksList)
        {
            var max = weeksList.Count == 0 ? 0 : weeksList.Max(w => w.TotalHours);
            return Math.Max(50, (int)Math.Ceiling(max / 5) * 5);
        }

        private static List<object[]> GetJson(IEnumerable<Week> weeks, decimal? defaultValue)
        {
            List<object[]> googleFormatedWeeks = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Uge nummer", Type = "string"},
                    new Column {Label = "Timer uden for kvartalet", Type = "number"},
                    new Column {Role = "style"},
                    new Column {Label = "HolidayHours", Type = "number"},
                    new Column {Label = "Work,Ferie,Sygdom osv.", Type = "number"},
                    new Column {Label = "Interessetid : 0% løn", Type = "number"},
                    new Column {Label = "39-44 : 100% løn", Type = "number"},
                    new Column {Label = "44+ : 150% løn", Type = "number"}
                }
            };
            googleFormatedWeeks.AddRange(weeks.Select(week => week.ToArray(defaultValue)));
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