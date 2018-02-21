using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Impact.Business.Holiday;
using Impact.Business.Time;
using Impact.Core.Contants;
using Impact.Core.Model;
using Impact.Website.Models;
using Impact.Website.Providers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly ITimeService _timeService;

        public AnalyzerController(ITimeService timeService)
        {
            _timeService = timeService;
        }

	    // GET: Analyzer
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            var quarterViewModel = CreateViewModels(DateTime.Now, token);
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var model = viewModel;

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarterViewModel = CreateViewModels(dateTime, token, viewModel.WeeksChartViewModel.IsNormalized);
            return View(quarterViewModel);
        }
        
        private QuarterViewModel CreateViewModels(DateTime dateTime, SecurityToken token, bool isNormalized = false)
        {
            var quarter = _timeService.GetQuarter(dateTime);
            var weeks = _timeService.GetWeeksInQuarter(quarter, token).ToList();
            var normalizedWeeks = _timeService.GetNormalizedWeeks(weeks).ToList();
            
            var interestHoursSum = normalizedWeeks.Sum(w => w.InterestHours);
            var moveableOvertimeHours = normalizedWeeks.Sum(w => w.MoveableOvertimeHours);

            var percentile = (ApplicationConstants.MoveableConst / ApplicationConstants.InterestConst);
            
            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);

            quarterViewModel.BalanceChartViewModel = CreateBalanceViewModel(weeks);
            quarterViewModel.WeeksChartViewModel = WeeksChartViewModelProvider.CreateWeeksViewModel(quarter, weeks, normalizedWeeks, isNormalized);
            quarterViewModel.PieChartViewModel = CreatePieChartViewModel(normalizedWeeks);
            var round = interestHoursSum == 0 ? 100 : Math.Round(moveableOvertimeHours / interestHoursSum / percentile * 100);
            var potentialOptions = new GaugeChartViewModel.OptionsViewModel(0, 33, 66, 100);
            quarterViewModel.PotentialChartViewModel = new GaugeChartViewModel("potential_chart", potentialOptions, CreateGaugeJson("Potentiale", round));
            
            return quarterViewModel;
        }

        private BalanceChartViewModel CreateBalanceViewModel(List<Week> weeks)
        {
            var previousWeeks = weeks.Where(w => !w.Dates.Contains(DateTime.Today)).ToList();
            var normalizedWeeks = _timeService.GetNormalizedWeeks(previousWeeks).ToList();
            var sum1 = normalizedWeeks.Sum(w => w.InterestHours + w.MoveableOvertimeHours);
            var sum2 = normalizedWeeks.Sum(w => ApplicationConstants.NormalWorkWeek - (w.WorkHours + w.HolidayHours + w.QuarterEdgeHours));
            var sum = sum1 - sum2;

            int ceiling = (int)Math.Ceiling(sum >= 0 ? sum : sum / 10);
            int dynamicXMax = ceiling * 10;
            int xMax = Math.Max(10, dynamicXMax < 0 ? dynamicXMax * -1 : dynamicXMax);

            List<object[]> googleFormatedBalance = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Timer", Type = "number"},
                     
                }
            };
            googleFormatedBalance.Add(new object[] {"Saldo", sum});
            
            var balanceViewModel = new BalanceChartViewModel();
            balanceViewModel.DivId = "balance_chart";
            balanceViewModel.Title = "Time saldo";
            balanceViewModel.SubTitle = "Viser din \"time-saldo\" for dette kvartal. Dette er summen af dine flytbare timer (interessetid + 39-44 overarbejde) minus dine manglende timer (hvis du er gået tidligt hjem en uge)\nKort sagt: Er grafen i minus skal du arbejde længere en uge. Er grafen i plus kan du gå tidligt hjem en uge";
            balanceViewModel.Color = sum >= 0 ? ApplicationConstants.Color.Blue : ApplicationConstants.Color.Black;
            balanceViewModel.XMax = xMax;
            balanceViewModel.XMin = -1 * xMax;
            balanceViewModel.Json = googleFormatedBalance;
            return balanceViewModel;
        }

        private PieChartViewModel CreatePieChartViewModel(List<Week> normalizedWeeks)
        {
            var interestHoursSum = normalizedWeeks.Sum(w => w.InterestHours);
            var moveableOvertimeHours = normalizedWeeks.Sum(w => w.MoveableOvertimeHours);

            List<object[]> googleFormatedPie = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Hour Type", Type = "string"},
                    new Column {Label = "Hours in Quarter", Type = "number"}
                }
            };

            googleFormatedPie.Add(new object[] {"Interessetid : 0% løn", interestHoursSum});
            googleFormatedPie.Add(new object[] {"39-44 : 100% løn", moveableOvertimeHours});
            
            var pieChartViewModel = new PieChartViewModel();
            pieChartViewModel.DivId = "pie_chart";
            pieChartViewModel.Title = "Interessetid vs Overarbejde";
            pieChartViewModel.Json = googleFormatedPie;
            
            return pieChartViewModel;
        }

        private List<object[]> CreateGaugeJson(string title, decimal value)
        {
            List<object[]> googleFormatedGauge = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Label", Type = "string"},
                    new Column {Label = "Value", Type = "number"}
                }
            };
            
            googleFormatedGauge.Add(new object[] {title, value});
            
            return googleFormatedGauge;
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
    }
}