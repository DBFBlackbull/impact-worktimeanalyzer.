using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.Website.Models;
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

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            if (!profile.IsDeveloper)
                return RedirectToAction("Index", "Site");

            var dateTime = DateTime.Now;
            var quarter = _timeService.GetQuarter(dateTime);
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, profile, token);
            quarterViewModel.ShowIncludeAllWeeksButton = true;
            return View(quarterViewModel);
        }

        [HttpPost]
        public ActionResult Index(QuarterViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            if (!profile.IsDeveloper)
                return RedirectToAction("Index", "Site");
            
            if (!ModelState.IsValid)
                return View(viewModel);

            var dateTime = DateTime.Parse(viewModel.SelectedQuarter);
            var quarter = _timeService.GetQuarter(dateTime);
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, profile, token, viewModel.BarColumnChartViewModel.IsNormalized);
            quarterViewModel.ShowIncludeAllWeeksButton = quarterViewModel.Quarters.Last().Selected;
            return View(quarterViewModel);
        }
    }
}