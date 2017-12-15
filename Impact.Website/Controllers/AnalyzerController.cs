using System;
using System.Collections.Generic;
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
            var quarterViewModel = CreateViewModels(dateTime, token, viewModel.WeeksViewModel.IsNormalized);
            return View(quarterViewModel);
        }
        
        private QuarterViewModel CreateViewModels(DateTime dateTime, SecurityToken token, bool isNormalized = false)
        {
            var quarter = _timeService.GetQuarter(dateTime);
            var weeks = _timeService.GetWeeksInQuarter(quarter, token).ToList();
            var normalizedWeeks = _timeService.GetNormalizedWeeks(weeks).ToList();
            
            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);

            BalanceViewModel balanceViewModel = CreateBalanceViewModel(normalizedWeeks);
            WeeksViewModel weeksViewModel = WeeksViewModelProvider.CreateWeeksViewModel(quarter, weeks, normalizedWeeks, isNormalized);

            quarterViewModel.BalanceViewModel = balanceViewModel;
            quarterViewModel.WeeksViewModel = weeksViewModel;
            
            return quarterViewModel;
        }

        private BalanceViewModel CreateBalanceViewModel(List<Week> normalizedWeeks)
        {
            var sum = normalizedWeeks.Sum(w => w.InterestHours + w.MoveableOvertimeHours) -
                      normalizedWeeks.Sum(w => w.WorkHours + w.HolidayHours - ApplicationConstants.NormalWorkWeek);

            List<object[]> googleFormatedBalance = new List<object[]>
            {
                new object[]
                {
                    new Column{Label = "", Type = "string"},
                    new Column{Label = "Sum", Type = "number"},
                     
                }
            };
            googleFormatedBalance.Add(new object[] {"Sum", sum});
            
            var balanceViewModel = new BalanceViewModel();
            balanceViewModel.Title = "Over/Under";
            balanceViewModel.SubTitle = "Interessetid + MoveableHours - MissingHours";
            balanceViewModel.Json = googleFormatedBalance;
            balanceViewModel.Color = sum >= 0 ? "green" : "red";
            return balanceViewModel;
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

                SelectListGroup group;
                if (!groupsMap.TryGetValue(currentYear, out group))
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