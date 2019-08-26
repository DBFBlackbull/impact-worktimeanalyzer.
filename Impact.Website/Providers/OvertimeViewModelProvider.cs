using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Model;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using Impact.Website.Models.Options;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Providers
{
    public static class OvertimeViewModelProvider
    {
        public static QuarterViewModel CreateViewModels(ITimeService timeService, Quarter quarter, SecurityToken token, bool isNormalized = false, List<Week> rawWeeksOverride = null)
        {
            List<Week> rawWeeks = rawWeeksOverride ?? timeService.GetWeeksInQuarter(quarter, token).ToList();

            var now = DateTime.Today;

            var previousWeeks = rawWeeks.Where(w => w.Dates.LastOrDefault() < now).ToList();
            var normalizedPreviousWeek = timeService.GetNormalizedWeeks(previousWeeks).ToList();
            var normalizedAllWeeks = timeService.GetNormalizedWeeks(rawWeeks).ToList();

            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.OvertimePaycheck = quarter.GetDisplayOvertimePayoutMonth();

            quarterViewModel.BalanceChartViewModel = CreateBalanceViewModel(normalizedPreviousWeek, normalizedAllWeeks); 
            quarterViewModel.PieChartViewModel = CreatePieChartViewModel(normalizedPreviousWeek, normalizedAllWeeks); 
            quarterViewModel.PotentialChartViewModel = CreateGaugeChartViewModel(normalizedPreviousWeek, normalizedAllWeeks);
            quarterViewModel.SummedViewModel = CreateSummedViewModel(rawWeeks, normalizedPreviousWeek, normalizedAllWeeks);

            if (normalizedPreviousWeek.Count < rawWeeks.Count)
            {
                var count = rawWeeks.Count - normalizedPreviousWeek.Count;
                normalizedPreviousWeek.AddRange(rawWeeks.GetRange(normalizedPreviousWeek.Count, count));
            }
            
            quarterViewModel.BarColumnChartViewModel = CreateWeeksViewModel(quarter, rawWeeks, normalizedPreviousWeek, normalizedAllWeeks, isNormalized);
            
            return quarterViewModel;
        }
        
        public static BarColumnChartViewModel CreateWeeksViewModel(Quarter quarter, List<Week> weeksList,
            List<Week> normalizedPreviousWeek, List<Week> normalizedWeeks, bool isNormalized)
        {
            List<object[]> GetDataArray(IEnumerable<Week> weeks, decimal? defaultValue)
            {
                List<object[]> googleFormattedWeeks = new List<object[]>
                {
                    new object[]
                    {
                        new Column {Label = "Uge nummer", Type = "string"},
                        new Column {Label = "Timer uden for kvartalet", Type = "number"}, new Column {Role = "style"},
                        new Column {Label = "Timer fra helligdage", Type = "number"},
                        new Column {Label = "Arbejde, ferie, sygdom", Type = "number"},
                        new Column {Label = "Flex (37,5-39): 0% løn", Type = "number"},
                        new Column {Label = "Flex (39-44): 100% løn", Type = "number"},
                        new Column {Label = "Udbetalt (44+): 150% løn", Type = "number"}
                    }
                };
                googleFormattedWeeks.AddRange(weeks.Select(week => week.ToArray(defaultValue)));
                return googleFormattedWeeks;
            }

            var weeksViewModel = new BarColumnChartViewModel();
            weeksViewModel.DivId = "weeks_chart";
            weeksViewModel.IsNormalized = isNormalized;
            weeksViewModel.RawData = GetDataArray(weeksList, null);
            weeksViewModel.NormalizedPreviousData = GetDataArray(normalizedPreviousWeek, 0);
            weeksViewModel.NormalizedAllData = GetDataArray(normalizedWeeks, 0);
            
            var max = weeksList.Count == 0 ? 0 : weeksList.Max(w => w.TotalHours);
            var yAxisMax = Math.Max(50, (int)Math.Ceiling(max / 5) * 5);

            var vAxisViewModel = new BarColumnOptions.AxisViewModel
            {
                Title = "Timer",
                Ticks = new List<decimal> {2.5M, 5M, 7.5M, 10M, 12.5M, 15M, 17.5M, 20M, 22.5M, 25M, 27.5M, 30M, 32.5M, 35M, 37.5M, 40M, 42.5M, 45M, 47.5M, 50M, 52.5M, 55M, 57.5M, 60M, 62.5M, 65M, 67.5M, 70M, 72.5M, 75M, 77.5M, 80M, 82.5M, 85M, 87.5M, 90M, 92.5M, 95M, 97.5M, 100},
                ViewWindow = new BarColumnOptions.AxisViewModel.ViewWindowViewModel
                {
                    Min = 0,
                    Max = yAxisMax
                }
            };

            var optionsViewModel = new BarColumnOptions.OptionsViewModel
            {
                Width = 1600,
                Height = 800,
                IsStacked = true,
                Title = quarter.GetDisplayTitle(),
                Colors = new List<string> 
                {
                    ApplicationConstants.Color.White, 
                    ApplicationConstants.Color.LightBlue, 
                    ApplicationConstants.Color.Blue, 
                    ApplicationConstants.Color.Red, 
                    ApplicationConstants.Color.Orange,
                    ApplicationConstants.Color.Green
                },
                Animation = new BarColumnOptions.OptionsViewModel.AnimationViewModel
                {
                    Duration = 1000,
                    Easing = AnimationEasing.Out
                },
                Bar = new BarColumnOptions.OptionsViewModel.BarViewModel
                {
                    GroupWidth = 50
                },
                VAxis = vAxisViewModel,
            };

            weeksViewModel.Options = optionsViewModel;
            return weeksViewModel;
        }
        public static PieChartViewModel CreatePieChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            var interestHoursSumPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours);
            var movableOvertimeHoursPrevious = normalizedPreviousWeek.Sum(w => w.MovableOvertimeHours);

            List<object[]> previousWeeksData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Hour Type", Type = "string"},
                    new Column {Label = "Hours in Quarter", Type = "number"}
                }
            };

            const string flexZeroPercent = "Flex (37,5-39) : 0% løn";
            const string flex100Percent = "Flex (39-44) : 0% løn";
            
            previousWeeksData.Add(new object[] {flexZeroPercent, interestHoursSumPrevious});
            previousWeeksData.Add(new object[] {flex100Percent, movableOvertimeHoursPrevious});
            
            var interestHoursSumAll = normalizedAllWeeks.Sum(w => w.InterestHours);
            var movableOvertimeHoursAll = normalizedAllWeeks.Sum(w => w.MovableOvertimeHours);

            List<object[]> allWeeksData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Hour Type", Type = "string"},
                    new Column {Label = "Hours in Quarter", Type = "number"}
                }
            };

            allWeeksData.Add(new object[] {flexZeroPercent, interestHoursSumAll});
            allWeeksData.Add(new object[] {flex100Percent, movableOvertimeHoursAll});

            var optionViewModel = new PieChartViewModel.OptionViewModel
            {
                Title = "0% løn vs 100% løn",
                Colors = new List<string> {ApplicationConstants.Color.Red, ApplicationConstants.Color.Orange}
            };

            var pieChartViewModel = new PieChartViewModel();
            pieChartViewModel.DivId = "pie_chart";
            pieChartViewModel.Options = optionViewModel;
            pieChartViewModel.NormalizedPreviousData = previousWeeksData;
            pieChartViewModel.NormalizedAllData = allWeeksData;
            
            return pieChartViewModel;
        }
        
        public static GaugeChartViewModel CreateGaugeChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            const decimal percentile = ApplicationConstants.MovableConst / ApplicationConstants.InterestConst;
            
            var interestHoursSumPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours);
            var movableOvertimeHoursPrevious = normalizedPreviousWeek.Sum(w => w.MovableOvertimeHours);
            
            var percentPrevious = interestHoursSumPrevious == 0 ? 100 : Math.Round(movableOvertimeHoursPrevious / interestHoursSumPrevious / percentile * 100);
            List<object[]> previousWeeksData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Label", Type = "string"},
                    new Column {Label = "Value", Type = "number"}
                }
            };
            var gaugeTitle = "Potentiale";
            previousWeeksData.Add(new object[] {gaugeTitle, percentPrevious});
            
            var interestHoursSumAll = normalizedAllWeeks.Sum(w => w.InterestHours);
            var movableOvertimeHoursAll = normalizedAllWeeks.Sum(w => w.MovableOvertimeHours);
            var percentAll = interestHoursSumAll == 0 ? 100 : Math.Round(movableOvertimeHoursAll / interestHoursSumAll / percentile * 100);
            List<object[]> allWeeksData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Label", Type = "string"},
                    new Column {Label = "Value", Type = "number"}
                }
            };
            allWeeksData.Add(new object[] {gaugeTitle, percentAll});
            
            var potentialOptions = new GaugeChartViewModel.OptionsViewModel(0, 33, 66, 100);
            var potentialChartViewModel = new GaugeChartViewModel
            {
                DivId = "potential_chart", 
                NormalizedPreviousData = previousWeeksData,
                NormalizedAllData = allWeeksData,
                Options = potentialOptions, 
            };
            return potentialChartViewModel;
        }
        
        public static BarColumnChartViewModel CreateBalanceViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            var overHoursPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours + w.MovableOvertimeHours);
            var missingHoursPrevious = normalizedPreviousWeek.Sum(w => ApplicationConstants.NormalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
            var balanceHoursPrevious = overHoursPrevious - missingHoursPrevious;

            var overHoursAll = normalizedAllWeeks.Sum(w => w.InterestHours + w.MovableOvertimeHours);
            var missingHoursAll = normalizedAllWeeks.Sum(w => ApplicationConstants.NormalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
            var balanceHoursAll = overHoursAll - missingHoursAll;

            var maxBalance = Math.Max(Math.Abs(balanceHoursPrevious), Math.Abs(balanceHoursAll));
            int dynamicXMax = (int) Math.Ceiling(maxBalance / 5) * 5;
            int xMax = Math.Max(10, dynamicXMax);

            List<object[]> previousData = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Timer", Type = "number"},
                     
                }
            };
            previousData.Add(new object[] {"Saldo", balanceHoursPrevious});
            
            List<object[]> allData = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Flex", Type = "number"},
                     
                }
            };
            allData.Add(new object[] {"Flex", balanceHoursAll});
            
            var color = balanceHoursPrevious >= 0 ? ApplicationConstants.Color.Blue : ApplicationConstants.Color.Black;

            var options = new BarColumnOptions.MaterialOptionsViewModel()
            {
                Height = 170,
                Colors = new List<string> {color},
                Bars = BarOrientation.Horizontal,
                HAxis = new BarColumnOptions.AxisViewModel
                {
                    ViewWindow = new BarColumnOptions.AxisViewModel.ViewWindowViewModel
                    {
                        Max = xMax,
                        Min = xMax * -1
                    }
                },
                Chart = new BarColumnOptions.MaterialOptionsViewModel.ChartViewModel
                {
                    Title = "Flex saldo",
                    Subtitle = "Viser dine Flex-timer for dette kvartal. Dette er summen af dine Flex-timer (Flex 37,5-39 + Flex 39-44) minus dine manglende timer (hvis du er gået tidligt hjem en uge)" +
                               "\nKort sagt: Er grafen i minus skal du arbejde længere en uge. Er grafen i plus kan du gå tidligt hjem en uge"
                }
            };

            var balanceViewModel = new BarColumnChartViewModel
            {
                DivId = "balance_chart",
                NormalizedPreviousData = previousData,
                NormalizedAllData = allData,
                Options = options
            };
            return balanceViewModel;
        }
        
        public static SummedViewModel CreateSummedViewModel(List<Week> rawWeeks, List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            SummedViewModel.Data Func(List<Week> weeks) => new SummedViewModel.Data
            {
                Flex0 = weeks.Sum(w => w.InterestHours), 
                Flex100 = weeks.Sum(w => w.MovableOvertimeHours), 
                Payout = weeks.Sum(w => w.LockedOvertimeHours),
            };

            return new SummedViewModel
            {
                RawAll = Func(rawWeeks),
                NormalizedPrevious = Func(normalizedPreviousWeek),
                NormalizedAll = Func(normalizedAllWeeks),
            };
        }
    }
}