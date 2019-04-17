using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Contants;
using Impact.Core.Model;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using Impact.Website.Models.Options;
using Impact.Website.Providers;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class OvertimeController : Controller
    {
        private readonly ITimeService _timeService;

        public OvertimeController(ITimeService timeService)
        {
            _timeService = timeService;
        }

	    // GET: Analyzer
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            var quarterViewModel = CreateViewModels(DateTime.Now, token);
            quarterViewModel.ShowIncludeAllWeeksButton = true;
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarterViewModel = CreateViewModels(dateTime, token, viewModel.BarColumnChartViewModel.IsNormalized);
            quarterViewModel.ShowIncludeAllWeeksButton = quarterViewModel.Quarters.Last().Selected;
            return View(quarterViewModel);
        }
        
        private QuarterViewModel CreateViewModels(DateTime dateTime, SecurityToken token, bool isNormalized = false)
        {
            var quarter = _timeService.GetQuarter(dateTime);
            List<Week> rawWeeks = _timeService.GetWeeksInQuarter(quarter, token).ToList();

            var now = DateTime.Today;

            var previousWeeks = rawWeeks.Where(w => w.Dates.LastOrDefault() < now).ToList();
            var normalizedPreviousWeek = _timeService.GetNormalizedWeeks(previousWeeks).ToList();
            var normalizedAllWeeks = _timeService.GetNormalizedWeeks(rawWeeks).ToList();

            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);

            quarterViewModel.BalanceChartViewModel = CreateBalanceViewModel(normalizedPreviousWeek, normalizedAllWeeks); 
            quarterViewModel.PieChartViewModel = CreatePieChartViewModel(normalizedPreviousWeek, normalizedAllWeeks); 
            quarterViewModel.PotentialChartViewModel = CreateGaugeChartViewModel(normalizedPreviousWeek, normalizedAllWeeks);
            quarterViewModel.SummedViewModel = CreateSummedViewModel(rawWeeks, normalizedPreviousWeek, normalizedAllWeeks);

            if (normalizedPreviousWeek.Count < rawWeeks.Count)
            {
                var count = rawWeeks.Count - normalizedPreviousWeek.Count;
                normalizedPreviousWeek.AddRange(rawWeeks.GetRange(normalizedPreviousWeek.Count, count));
            }
            
            quarterViewModel.BarColumnChartViewModel = WeeksChartViewModelProvider.CreateWeeksViewModel(quarter, rawWeeks, normalizedPreviousWeek, normalizedAllWeeks, isNormalized);
            
            return quarterViewModel;
        }

        private static BarColumnChartViewModel CreateBalanceViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            var overHoursPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours + w.MoveableOvertimeHours);
            var missingHoursPrevious = normalizedPreviousWeek.Sum(w => ApplicationConstants.NormalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
            var balanceHoursPrevious = overHoursPrevious - missingHoursPrevious;

            var overHoursAll = normalizedAllWeeks.Sum(w => w.InterestHours + w.MoveableOvertimeHours);
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
                Chart =new BarColumnOptions.MaterialOptionsViewModel.ChartViewModel
                {
                    Title = "Flex saldo",
                    Subtitle = "Viser dine Flex-timer for dette kvartal. Dette er summen af dine Flex-timer (Flex 37,5-39 + Flex 39-44) minus dine manglende timer (hvis du er gået tidligt hjem en uge)" +
                               "\nKort sagt: Er grafen i minus skal du arbejde længere en uge. Er grafen i plus kan du gå tidligt hjem en uge"
                }
            };

            var balanceViewModel = new BarColumnChartViewModel
            {
                DivId = "balance_chart",
                NormalizedPreviousWeeks = previousData,
                NormalizedAllWeeks = allData,
                Options = options
            };
            return balanceViewModel;
        }

        private static PieChartViewModel CreatePieChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            var interestHoursSumPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours);
            var moveableOvertimeHoursPrevious = normalizedPreviousWeek.Sum(w => w.MoveableOvertimeHours);

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
            previousWeeksData.Add(new object[] {flex100Percent, moveableOvertimeHoursPrevious});
            
            var interestHoursSumAll = normalizedAllWeeks.Sum(w => w.InterestHours);
            var moveableOvertimeHoursAll = normalizedAllWeeks.Sum(w => w.MoveableOvertimeHours);

            List<object[]> allWeeksData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Hour Type", Type = "string"},
                    new Column {Label = "Hours in Quarter", Type = "number"}
                }
            };

            allWeeksData.Add(new object[] {flexZeroPercent, interestHoursSumAll});
            allWeeksData.Add(new object[] {flex100Percent, moveableOvertimeHoursAll});

            var optionViewModel = new PieChartViewModel.OptionViewModel
            {
                Title = "0% løn vs 100% løn",
                Colors = new List<string> {ApplicationConstants.Color.Red, ApplicationConstants.Color.Orange}
            };

            var pieChartViewModel = new PieChartViewModel();
            pieChartViewModel.DivId = "pie_chart";
            pieChartViewModel.Options = optionViewModel;
            pieChartViewModel.NormalizedPreviousWeeks = previousWeeksData;
            pieChartViewModel.NormalizedAllWeeks = allWeeksData;
            
            return pieChartViewModel;
        }
        
        private static GaugeChartViewModel CreateGaugeChartViewModel(List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            const decimal percentile = ApplicationConstants.MoveableConst / ApplicationConstants.InterestConst;
            
            var interestHoursSumPrevious = normalizedPreviousWeek.Sum(w => w.InterestHours);
            var moveableOvertimeHoursPrevious = normalizedPreviousWeek.Sum(w => w.MoveableOvertimeHours);
            
            var percentPrevious = interestHoursSumPrevious == 0 ? 100 : Math.Round(moveableOvertimeHoursPrevious / interestHoursSumPrevious / percentile * 100);
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
            var moveableOvertimeHoursAll = normalizedAllWeeks.Sum(w => w.MoveableOvertimeHours);
            var percentAll = interestHoursSumAll == 0 ? 100 : Math.Round(moveableOvertimeHoursAll / interestHoursSumAll / percentile * 100);
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
                NormalizedPreviousWeeks = previousWeeksData,
                NormalizedAllWeeks = allWeeksData,
                Options = potentialOptions, 
            };
            return potentialChartViewModel;
        }

        private IEnumerable<SelectListItem> GetSelectList(Quarter quarter)
        {
            var startDate = DateTime.Now;
            var selectListItems = new List<SelectListItem>();
            var groupsMap = new Dictionary<int, SelectListGroup>();

            for (int i = 16 - 1; i >= 0; i--)
            {
                var date = startDate.AddMonths(i * -3);
                var selectQuarter = _timeService.GetQuarter(date);
                var currentDate = selectQuarter.MidDate;
                var currentYear = currentDate.Year;

                if (!groupsMap.TryGetValue(currentYear, out var group))
                    group = groupsMap[currentYear] = new SelectListGroup { Name = currentYear.ToString() };

                selectListItems.Add(new SelectListItem
                {
                    Group = group,
                    Selected = quarter.MidDate == currentDate,
                    Value = currentDate.ToShortDateString(),
                    Text = selectQuarter.GetDisplayText() + " " + currentYear
                });
            }

            return selectListItems;
        }

        private static SummedViewModel CreateSummedViewModel(List<Week> rawWeeks, List<Week> normalizedPreviousWeek, List<Week> normalizedAllWeeks)
        {
            SummedViewModel.Data Func(List<Week> weeks) => new SummedViewModel.Data
            {
                Flex0 = weeks.Sum(w => w.InterestHours), 
                Flex100 = weeks.Sum(w => w.MoveableOvertimeHours), 
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