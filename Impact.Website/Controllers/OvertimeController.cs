using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Constants;
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
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            if (!(bool) HttpContext.Session[ApplicationConstants.SessionName.IsDeveloper])
                return RedirectToAction("Index", "Site");

            var quarter = _timeService.GetQuarter(DateTime.Now);
            var quarterViewModel = OvertimeViewModelProvider.CreateViewModels(_timeService, quarter, token);
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.ShowIncludeAllWeeksButton = true;
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            if (!(bool) HttpContext.Session[ApplicationConstants.SessionName.IsDeveloper])
                return RedirectToAction("Index", "Site");
            
            if (!ModelState.IsValid)
                return View(viewModel);

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarter = _timeService.GetQuarter(dateTime);
            var quarterViewModel = OvertimeViewModelProvider.CreateViewModels(_timeService, quarter, token, viewModel.BarColumnChartViewModel.IsNormalized);
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.ShowIncludeAllWeeksButton = quarterViewModel.Quarters.Last().Selected;
            return View(quarterViewModel);
        }
        
        private IEnumerable<SelectListItem> GetSelectList(Quarter quarter)
        {
            var startDate = DateTime.Now;
            var selectListItems = new List<SelectListItem>();
            var groupsMap = new Dictionary<int, SelectListGroup>();

            for (int i = 15; i >= 0; i--)
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
                    Text = selectQuarter.GetDisplayTitle() + " " + currentYear
                });
            }

            return selectListItems;
        }
    }
}