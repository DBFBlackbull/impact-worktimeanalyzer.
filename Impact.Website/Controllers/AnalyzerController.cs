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
                return RedirectToAction("Index", "Home");
            
            var quarterViewModel = CreateViewModels(DateTime.Now, token);
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Home");

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

            quarterViewModel.BalanceChartViewModel = CreateBalanceViewModel(normalizedWeeks);
            quarterViewModel.WeeksChartViewModel = WeeksChartViewModelProvider.CreateWeeksViewModel(quarter, weeks, normalizedWeeks, isNormalized);
            quarterViewModel.PieChartViewModel = CreatePieChartViewModel(normalizedWeeks);
            var round = interestHoursSum == 0 ? 0 : Math.Round(moveableOvertimeHours / interestHoursSum / percentile * 100);
            var potentialOptions = new GaugeChartViewModel.OptionsViewModel(0, 33, 66, 100);
            quarterViewModel.PotentialChartViewModel = new GaugeChartViewModel("potential_chart", potentialOptions, CreateGaugeJson("Potentiale", round));
            
            return quarterViewModel;
        }

        private BalanceChartViewModel CreateBalanceViewModel(List<Week> normalizedWeeks)
        {
            var sum = normalizedWeeks.Sum(w => w.InterestHours + w.MoveableOvertimeHours) -
                      normalizedWeeks.Sum(w => w.WorkHours + w.HolidayHours - ApplicationConstants.NormalWorkWeek) - 1;

            var max = (int)Math.Ceiling(sum / 5) * 5;

            List<object[]> googleFormatedBalance = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Sum", Type = "number"},
                     
                }
            };
            googleFormatedBalance.Add(new object[] {"Sum", sum});
            
            var balanceViewModel = new BalanceChartViewModel();
            balanceViewModel.DivId = "balance_chart";
            balanceViewModel.Title = "Over/Under";
            balanceViewModel.SubTitle = "Interessetid + MoveableHours - MissingHours";
            balanceViewModel.Color = sum >= 0 ? ApplicationConstants.Color.Blue : ApplicationConstants.Color.Black;
            balanceViewModel.XMax = max;
            balanceViewModel.XMin = -1 * max;
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

//        public ActionResult Demo(string id)
//        {
//            List<Week> weeks;
//            QuarterViewModel quarterViewModel;
//            switch (id)
//            {
//                case "1":
//                    weeks = new List<Week>
//                    {
//                        new Week
//                        {
//                            Number = 1,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 2,
//                            TotalHours = 36
//                        },
//                        new Week
//                        {
//                            Number = 3,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 4,
//                            TotalHours = 36
//                        },
//                        new Week
//                        {
//                            Number = 5,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 6,
//                            TotalHours = 36
//                        },
//                    };
//
//                    quarterViewModel = CreateDemoViewModel(weeks);
//                    return View("Index", quarterViewModel);
//                case "2":
//                    weeks = new List<Week>
//                    {
//                        new Week
//                        {
//                            Number = 1,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 2,
//                            TotalHours = 36
//                        },
//                        new Week
//                        {
//                            Number = 3,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 4,
//                            TotalHours = 36
//                        },
//                        new Week
//                        {
//                            Number = 5,
//                            TotalHours = 39
//                        },
//                        new Week
//                        {
//                            Number = 6,
//                            TotalHours = 36
//                        },
//                        new Week
//                        {
//                            Number = 7,
//                            TotalHours = 45
//                        },
//                    };
//
////                    quarterViewModel = CreateDemoViewModel(weeks);
//                    return View("Index", quarterViewModel);
//            }
//
//            return Content("test");
//        }
        
//        public QuarterViewModel CreateDemoViewModel(List<Week> weeks)
//        {
//            var quarter = _timeService.GetQuarter();
//            _holidayService.AddHolidayHours(quarter, weeks);
//            weeks.ForEach(week => week.CategorizeHours());
//
//            var normalizedWeeks = CreateNormalizedWeeks(weeks).ToList();
//
//            var quarterViewModel = CreateWeeksViewModel(quarter, weeks);
//            quarterViewModel.NormalizedJson = GetJson(normalizedWeeks);
//            return quarterViewModel;
//        }
    }
}