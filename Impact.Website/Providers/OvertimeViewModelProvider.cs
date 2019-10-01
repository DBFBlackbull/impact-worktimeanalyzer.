using System;
using System.Collections.Generic;
using System.Linq;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using Impact.Website.Models.Options;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Providers
{
    public class OvertimeViewModelProvider
    {
        private readonly ITimeService _timeService;
        private readonly ITimeRepository _timeRepository;
        private string _flexZero;
        private string _flex100;
        private string _payout;
        private string _flexZeroPercent;
        private string _flex100Percent;
        private string _payoutPercent;

        public OvertimeViewModelProvider(ITimeService timeService, ITimeRepository timeRepository)
        {
            _timeService = timeService;
            _timeRepository = timeRepository;
        }

        public QuarterViewModel CreateViewModels(Quarter quarter, Profile profile, SecurityToken token, bool isNormalized = false, List<Week> rawWeeksOverride = null)
        {
            var rawWeeks = rawWeeksOverride ?? _timeRepository.GetRawWeeksInQuarter(quarter, token).ToList();
            rawWeeks = _timeService.CategorizeWeeks(quarter, rawWeeks, profile.NormalWorkDay, token).ToList();

            var interestHoursLimit = (profile.NormalWorkWeek + ApplicationConstants.InterestConst).Normalize();
            var movableHoursLimit = (interestHoursLimit + ApplicationConstants.MovableConst).Normalize();
            _flexZero = $"Flex ({profile.NormalWorkWeek.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat)}-{interestHoursLimit.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat)})";
            _flex100 = $"Flex ({interestHoursLimit.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat)}-{movableHoursLimit.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat)})";
            _payout = $"Udbetalt ({movableHoursLimit.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat)}+)";

            _flexZeroPercent = $"{_flexZero}: 0% løn";
            _flex100Percent = $"{_flex100}: 100% løn";
            _payoutPercent = $"{_payout}: 150% løn";
            
            var now = DateTime.Today;
            var previousWeeks = rawWeeks.Where(w => w.Dates.LastOrDefault(d => d <= quarter.To) < now).ToList();
            var normalizedPreviousWeek = _timeService.GetNormalizedWeeks(previousWeeks, profile).ToList();
            var normalizedAllWeeks = _timeService.GetNormalizedWeeks(rawWeeks, profile).ToList();

            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.From.ToShortDateString();
            quarterViewModel.NormalWorkMonth = profile.NormalWorkMonth;
            quarterViewModel.DisplayNormalWorkWeek = profile.NormalWorkWeek.ToString(ApplicationConstants.DanishCultureInfo.NumberFormat);
            quarterViewModel.DisplayFlexZero = _flexZero;
            quarterViewModel.DisplayFlex100 = _flex100;
            quarterViewModel.DisplayPayout = _payout;

            quarterViewModel.BalanceChartViewModel = CreateBalanceViewModel(normalizedPreviousWeek, normalizedAllWeeks, profile.NormalWorkWeek); 
            quarterViewModel.PieChartViewModel = CreatePieChartViewModel(normalizedPreviousWeek, normalizedAllWeeks); 
            quarterViewModel.PotentialChartViewModel = CreateGaugeChartViewModel(normalizedPreviousWeek, normalizedAllWeeks);
            quarterViewModel.SummedViewModel = CreateSummedViewModel(quarter, normalizedPreviousWeek, normalizedAllWeeks, profile.NormalWorkMonth);

            if (normalizedPreviousWeek.Count < rawWeeks.Count)
            {
                var count = rawWeeks.Count - normalizedPreviousWeek.Count;
                normalizedPreviousWeek.AddRange(rawWeeks.GetRange(normalizedPreviousWeek.Count, count));
            }
            
            quarterViewModel.BarColumnChartViewModel = CreateWeeksViewModel(quarter, rawWeeks, normalizedPreviousWeek, normalizedAllWeeks, isNormalized);
            
            return quarterViewModel;
        }

        private BarColumnChartViewModel CreateWeeksViewModel(Quarter quarter, List<Week> weeksList,
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
                        new Column {Label = _flexZeroPercent, Type = "number"},
                        new Column {Label = _flex100Percent, Type = "number"},
                        new Column {Label = _payoutPercent, Type = "number"}
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
                Title = $"{quarter.GetDisplayTitle()}: {quarter.GetDisplayMonths()}",
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

        private PieChartViewModel CreatePieChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
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

            previousWeeksData.Add(new object[] {_flexZeroPercent, interestHoursSumPrevious});
            previousWeeksData.Add(new object[] {_flex100Percent, movableOvertimeHoursPrevious});
            
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

            allWeeksData.Add(new object[] {_flexZeroPercent, interestHoursSumAll});
            allWeeksData.Add(new object[] {_flex100Percent, movableOvertimeHoursAll});

            var optionViewModel = new PieChartViewModel.OptionViewModel
            {
                Title = $"{_flexZeroPercent} VS {_flex100Percent}",
                Colors = new List<string> {ApplicationConstants.Color.Red, ApplicationConstants.Color.Orange},
                ChartArea = new PieChartViewModel.OptionViewModel.ChartAreaViewModel
                {
                    Left = "0",
                    Width = "90%",
                    Height = "70%"
                }
            };

            var pieChartViewModel = new PieChartViewModel();
            pieChartViewModel.DivId = "pie_chart";
            pieChartViewModel.Options = optionViewModel;
            pieChartViewModel.NormalizedPreviousData = previousWeeksData;
            pieChartViewModel.NormalizedAllData = allWeeksData;
            
            return pieChartViewModel;
        }

        private static GaugeChartViewModel CreateGaugeChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
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

        private BarColumnChartViewModel CreateBalanceViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks, decimal normalWorkWeek)
        {
            var overHoursPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours + w.MovableOvertimeHours);
            var missingHoursPrevious = normalizedPreviousWeek.Sum(w => normalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
            var balanceHoursPrevious = overHoursPrevious - missingHoursPrevious;

            var overHoursAll = normalizedAllWeeks.Sum(w => w.InterestHours + w.MovableOvertimeHours);
            var missingHoursAll = normalizedAllWeeks.Sum(w => normalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
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
            previousData.Add(new object[] {"Flex", balanceHoursPrevious});
            
            List<object[]> allData = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Timer", Type = "number"},
                     
                }
            };
            allData.Add(new object[] {"Flex", balanceHoursAll});
            
            var color = balanceHoursPrevious >= 0 ? ApplicationConstants.Color.Blue : ApplicationConstants.Color.Black;

            var options = new BarColumnOptions.MaterialOptionsViewModel()
            {
                Height = 195,
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
                    Subtitle = $"Viser summen af dine Flex-timer [{_flexZero} + {_flex100}], minus dine manglende timer, hvis du er gået tidligt hjem en uge." +
                               $"\nHvis du vælger af afspadsere dine Flex-timer, vil du ALTID først blive trukket i dine \"gule timer\" [{_flex100Percent}]." +
                               $"\nEr der ikke nok \"gule timer\" vil du blive trukket i dine \"røde timer\" [{_flexZeroPercent}]"
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

        private SummedViewModel CreateSummedViewModel(Quarter quarter, List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks, decimal normalWorkMonth)
        {
            SummedViewModel.Data Func(List<Week> weeks) => new SummedViewModel.Data
            {
                Flex0 = weeks.Sum(w => w.InterestHours), 
                Flex100 = weeks.Sum(w => w.MovableOvertimeHours), 
                Payout = weeks.Sum(w => w.LockedOvertimeHours),
            };

            return new SummedViewModel
            {
                PayoutMonth = quarter.GetDisplayOvertimePayoutMonth(),
                NormalizedPrevious = Func(normalizedPreviousWeek),
                NormalizedAll = Func(normalizedAllWeeks),
            };
        }
    }
}