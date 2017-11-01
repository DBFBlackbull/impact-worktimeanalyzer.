using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

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
            var dateTime = new DateTime(2017, 3, 15);
            var quarter = _timeService.GetQuarter(dateTime);

            var weeks = _timeService.GetWeeksInQuarter(quarter);
            if (weeks == null)
                return RedirectToAction("Index", "Home");
            
            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.GraphTitle = GraphTitle(quarter);
            quarterViewModel.From = quarter.From;
            quarterViewModel.To = quarter.To;
            quarterViewModel.Json = GetJson(weeks);

            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);

            var quarter = _timeService.GetQuarter(dateTime);

            var weeks = _timeService.GetWeeksInQuarter(quarter);
            if (weeks == null)
                return RedirectToAction("Index", "Home");

            var quarterViewModel = new QuarterViewModel();
            quarterViewModel.SelectedQuarter = quarter.MidDate.ToShortDateString();
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.GraphTitle = GraphTitle(quarter);
            quarterViewModel.From = quarter.From;
            quarterViewModel.To = quarter.To;
            quarterViewModel.Json = GetJson(weeks);

            return View(quarterViewModel);
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
                    new Column {Label = "Work", Type = "number"},
                    new Column {Label = "Interessetid", Type = "number"},
                    new Column {Label = "39-44", Type = "number"},
                    new Column {Label = "44+", Type = "number"}
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
    }

    public class Column
    {
        public string Label { get; set; }
        public string Type { get; set; }
    }
}