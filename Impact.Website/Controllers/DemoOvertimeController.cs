using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using Impact.Website.Providers;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using Profile = Impact.Core.Model.Profile;

namespace Impact.Website.Controllers
{
    public class DemoOvertimeController : Controller
    {
        private readonly ITimeService _timeService;
        private readonly OvertimeViewModelProvider _viewModelProvider;
        private readonly IMapper _mapper;

        public DemoOvertimeController(ITimeService timeService, OvertimeViewModelProvider viewModelProvider, IMapper mapper)
        {
            _timeService = timeService;
            _viewModelProvider = viewModelProvider;
            _mapper = mapper;
        }

        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            if (!profile.IsDeveloper)
                return RedirectToAction("Index", "Site");

            var quarter = _timeService.GetQuarter(DateTime.Now);
            HttpContext.Session[ApplicationConstants.SessionName.SelectedQuarter] = quarter;
            
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, profile, token, false, out var rawWeeks);
            quarterViewModel.ShowIncludeAllWeeksButton = true;
            var demoOvertimeViewModel = _mapper.Map<DemoOvertimeViewModel>(quarterViewModel);
            demoOvertimeViewModel.InputWeeks = rawWeeks;

            return View(demoOvertimeViewModel);
        }
        
        [HttpPost]
        public ActionResult Index(DemoOvertimeViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            if (!profile.IsDeveloper)
                return RedirectToAction("Index", "Site");
            
            if (!ModelState.IsValid)
                return View(viewModel);

            ModelState.Clear();
            QuarterViewModel quarterViewModel;
            List<Week> rawWeeks;
            
            var oldQuarter = HttpContext.Session.Get<Quarter>(ApplicationConstants.SessionName.SelectedQuarter);
            var newQuarter = _timeService.GetQuarter(DateTime.Parse(viewModel.SelectedQuarter));
            if (oldQuarter.Equals(newQuarter))
            {
                quarterViewModel = _viewModelProvider.CreateViewModels(oldQuarter, profile, token, viewModel.BarColumnChartViewModel.IsNormalized, out rawWeeks,viewModel.InputWeeks);
            }
            else
            {
                HttpContext.Session[ApplicationConstants.SessionName.SelectedQuarter] = newQuarter;
                quarterViewModel = _viewModelProvider.CreateViewModels(newQuarter, profile, token, viewModel.BarColumnChartViewModel.IsNormalized, out rawWeeks);
            }

            quarterViewModel.ShowIncludeAllWeeksButton = quarterViewModel.Quarters.Last().Selected;
            var demoOvertimeViewModel = _mapper.Map<DemoOvertimeViewModel>(quarterViewModel);
            demoOvertimeViewModel.InputWeeks = rawWeeks;

            return View(demoOvertimeViewModel);
        }
    }
}