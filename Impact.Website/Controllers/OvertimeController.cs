using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
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
        private readonly OvertimeViewModelProvider _viewModelProvider;

        public OvertimeController(ITimeService timeService, OvertimeViewModelProvider viewModelProvider)
        {
            _timeService = timeService;
            _viewModelProvider = viewModelProvider;
        }

	    // GET: Analyzer
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            if (!HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).IsDeveloper)
                return RedirectToAction("Index", "Site");

            var quarter = _timeService.GetQuarter(DateTime.Now);
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, token);
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.ShowIncludeAllWeeksButton = true;
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            if (!HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).IsDeveloper)
                return RedirectToAction("Index", "Site");
            
            if (!ModelState.IsValid)
                return View(viewModel);

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarter = _timeService.GetQuarter(dateTime);
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, token, viewModel.BarColumnChartViewModel.IsNormalized);
            quarterViewModel.Quarters = GetSelectList(quarter);
            quarterViewModel.ShowIncludeAllWeeksButton = quarterViewModel.Quarters.Last().Selected;
            return View(quarterViewModel);
        }
        
        private IEnumerable<SelectListItem> GetSelectList(Quarter quarter)
        {
            var hiredDate = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).HiredDate;
            var start = _timeService.GetQuarter(hiredDate).From;
            var selectListItems = new List<SelectListItem>();
            var groupsMap = new Dictionary<int, SelectListGroup>();

            var now = DateTime.Now;
            while (start < now)
            {
                var selectQuarter = _timeService.GetQuarter(start);
                var currentDate = selectQuarter.From;
                var currentYear = currentDate.Year;

                if (!groupsMap.TryGetValue(currentYear, out var group))
                    group = groupsMap[currentYear] = new SelectListGroup { Name = currentYear.ToString() };

                selectListItems.Add(new SelectListItem
                {
                    Group = group,
                    Selected = quarter.From == currentDate,
                    Value = currentDate.ToShortDateString(),
                    Text = selectQuarter.GetDisplayTitle() + " " + currentYear
                });
                
                start = start.AddMonths(3);
            }
            
            

            return selectListItems;
        }
    }
}