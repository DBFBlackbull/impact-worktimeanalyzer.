using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Core.Contants;
using Impact.Core.Model;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using Impact.Website.Models.Options;

namespace Impact.Website.Providers
{
    public class WeeksChartViewModelProvider
    {
        public static BarColumnChartViewModel CreateWeeksViewModel(Quarter quarter, List<Week> weeksList,
            List<Week> normalizedPreviousWeek, List<Week> normalizedWeeks, bool isNormalized)
        {
            var weeksViewModel = new BarColumnChartViewModel();
            weeksViewModel.DivId = "weeks_chart";
            weeksViewModel.IsNormalized = isNormalized;
            weeksViewModel.RawWeeks = GetJson(weeksList, null);
            weeksViewModel.NormalizedPreviousWeeks = GetJson(normalizedPreviousWeek, 0);
            weeksViewModel.NormalizedAllWeeks = GetJson(normalizedWeeks, 0);
            weeksViewModel.Options = GetOptions(quarter, weeksList);
            
            return weeksViewModel;
        }

        private static BarColumnOptions GetOptions(Quarter quarter, List<Week> weeksList)
        {
            var animationViewModel = new BarColumnOptions.OptionsViewModel.AnimationViewModel
            {
                Duration = 1000,
                Easing = AnimationEasing.Out
            };

            var barViewModel = new BarColumnOptions.OptionsViewModel.BarViewModel
            {
                GroupWidth = 50
            };

            var viewWindowViewModel = new BarColumnOptions.AxisViewModel.ViewWindowViewModel
            {
                Min = 0,
                Max = YAxisMax(weeksList)
            };

            var vAxisViewModel = new BarColumnOptions.AxisViewModel
            {
                Title = "Timer",
                Ticks = new List<decimal> {2.5M, 5M, 7.5M, 10M, 12.5M, 15M, 17.5M, 20M, 22.5M, 25M, 27.5M, 30M, 32.5M, 35M, 37.5M, 40M, 42.5M, 45M, 47.5M, 50M, 52.5M, 55M, 57.5M, 60M, 62.5M, 65M, 67.5M, 70M, 72.5M, 75M, 77.5M, 80M, 82.5M, 85M, 87.5M, 90M, 92.5M, 95M, 97.5M, 100},
                ViewWindow = viewWindowViewModel
            };

            var hAxisViewModel = new BarColumnOptions.AxisViewModel
            {
                Title = "Uge"
            };

            var optionsViewModel = new BarColumnOptions.OptionsViewModel
            {
                Width = 1600,
                Height = 800,
                IsStacked = true,
                Title = GraphTitle(quarter),
                Colors = new List<string> {ApplicationConstants.Color.White, ApplicationConstants.Color.LightBlue, 
                    ApplicationConstants.Color.Blue, ApplicationConstants.Color.Red, ApplicationConstants.Color.Orange,
                    ApplicationConstants.Color.Green
                },
                Animation = animationViewModel,
                Bar = barViewModel,
                VAxis = vAxisViewModel,
                HAxis = hAxisViewModel
            };

            return optionsViewModel;
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