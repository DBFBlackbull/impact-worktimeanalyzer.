using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Holiday;
using Impact.Business.Time;
using Impact.Core.Contants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.Website.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class AnalyzerController : Controller
    {
        private readonly ITimeService _timeService;
        private readonly IHolidayService _holidayService;

        public AnalyzerController(ITimeService timeService, IHolidayService holidayService)
        {
            _timeService = timeService;
            _holidayService = holidayService;
        }

	    // GET: Analyzer
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Home");
            
            var dateTime = new DateTime(2017, 2, 15);
            var quarter = _timeService.GetQuarter(dateTime);
            var weeks = _timeService.GetWeeksInQuarter(quarter, token);

            var weeksList = weeks.ToList();
            _holidayService.AddHolidayHours(quarter, weeksList);
            weeksList.ForEach(week => week.CategorizeHours());

            var normalizedWeeks = CreateNormalizedWeeks(weeksList);
            var normalizedJson = GetJson(normalizedWeeks);

            QuarterViewModel quarterViewModel = CreateViewModel(quarter, weeksList);
            quarterViewModel.NormalizedJson = normalizedJson;
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);

            if (!(HttpContext.Session[ApplicationConstants.Token] is SecurityToken token))
                return RedirectToAction("Index", "Home");
            
            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarter = _timeService.GetQuarter(dateTime);
            var weeks = _timeService.GetWeeksInQuarter(quarter, token);

            var weeksList = weeks.ToList();
            _holidayService.AddHolidayHours(quarter, weeksList);
            weeksList.ForEach(week => week.CategorizeHours());

            var normalizedWeeks = CreateNormalizedWeeks(weeksList);
            var normalizedJson = GetJson(normalizedWeeks);
            
            var quarterViewModel = CreateViewModel(quarter, weeksList);
            quarterViewModel.IsNormalized = viewModel.IsNormalized;
            quarterViewModel.NormalizedJson = normalizedJson;
            return View(quarterViewModel);
        }
        
        private QuarterViewModel CreateViewModel(Quarter quarter, IEnumerable<Week> weeksList)
        {
            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.GraphTitle = GraphTitle(quarter);
            quarterViewModel.From = quarter.From;
            quarterViewModel.To = quarter.To;
            quarterViewModel.Json = GetJson(weeksList);
            return quarterViewModel;
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
                    Text = DisplayQuarter(selectQuarter) + " " + currentYear
                });
            }

            return selectListItems;
        }

        private static string DisplayQuarter(Quarter quarter)
        {
            switch (quarter.Number)
            {
                case 1:
                    return "1. Kvartal";
                case 2:
                    return "2. Kvartal";
                case 3:
                    return "3. Kvartal";
                case 4:
                    return "4. Kvartal";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + quarter);
            }
        }

        private static string GraphTitle(Quarter quarter)
        {
            switch (quarter.Number)
            {
                case 1:
                    return DisplayQuarter(quarter) + ": Januar til Marts";
                case 2:
                    return DisplayQuarter(quarter) + ": April til Juni";
                case 3:
                    return DisplayQuarter(quarter) + ": Juli til September";
                case 4:
                    return DisplayQuarter(quarter) + ": Oktober til December";
                default:
                    throw new IndexOutOfRangeException("Quarter was now 1, 2, 3, or 4. Real value: " + quarter);
            }
        }

        private static string GetJson(IEnumerable<Week> weeks)
        {
            List<object[]> googleFormatedWeeks = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Uge nummer", Type = "string"},
                    new Column {Label = "HolidayHours", Type = "number"},
                    new Column {Label = "Work,Ferie,Sygdom osv.", Type = "number"},
                    new Column {Label = "Interessetid", Type = "number"},
                    new Column {Label = "39-44 : 100% løn", Type = "number"},
                    new Column {Label = "44+ : 150% løn", Type = "number"}
                }
            };
            googleFormatedWeeks.AddRange(weeks.Select(week => week.ToArray()));

            var jsonSerializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var serializeObject = JsonConvert.SerializeObject(googleFormatedWeeks, jsonSerializerSettings);

            var replace = serializeObject.Replace('"', '\'');
            return replace;
        }
        
        private static IEnumerable<Week> CreateNormalizedWeeks(List<Week> weeksList)
        {
            var weeks = weeksList.ConvertAll(w => w.Clone());

            var lowWeeks = weeks.Where(w => w.WorkHours + w.HolidayHours < ApplicationConstants.NormalWorkWeek);
            var moveableWeeks = weeks.Where(w => w.MoveableOvertimeHours > 0).ToList();
            MoveHours(lowWeeks, moveableWeeks, "MoveableOvertimeHours");
            
            lowWeeks = weeks.Where(w => w.WorkHours + w.HolidayHours < ApplicationConstants.NormalWorkWeek);
            var interestWeeks = weeks.Where(w => w.InterestHours > 0).ToList();
            MoveHours(lowWeeks, interestWeeks, "InterestHours");
            
            return weeks;
        }

        private static void MoveHours(IEnumerable<Week> lowWeeks, List<Week> moveableWeeks, string propertyName)
        {
            foreach (var lowWeek in lowWeeks)
            {
                var weeksAbsorbed = 0;
                foreach (var moveableWeek in moveableWeeks)
                {
                    var doneAbsorbing = lowWeek.AbsorbHours(moveableWeek, propertyName);
                    if (doneAbsorbing)
                        break;
                    weeksAbsorbed++;
                }
                moveableWeeks.RemoveRange(0, weeksAbsorbed);
            }
        }
    }

    public class Column
    {
        public string Label { get; set; }
        public string Type { get; set; }
    }
}