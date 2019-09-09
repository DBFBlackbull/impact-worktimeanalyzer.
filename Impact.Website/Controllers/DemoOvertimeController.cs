using System;
using System.Collections.Generic;
using System.Web.Mvc;
using AutoMapper;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
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

            if (!HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).IsDeveloper)
                return RedirectToAction("Index", "Site");
            
            List<Week> inputWeeks = new List<Week>();
            for (var i = 1; i < 14; i++)
            {
                inputWeeks.Add(new Week
                {
                    Number = i,
                    TotalHours = 37.5 
                });
            }
            
            var quarter = _timeService.GetQuarter(new DateTime(2017, 2, 15));
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, token, rawWeeksOverride: inputWeeks);
            quarterViewModel.Quarters = GetSelectList(quarter);
            var demoOvertimeViewModel = _mapper.Map<DemoOvertimeViewModel>(quarterViewModel);
            demoOvertimeViewModel.InputWeeks = inputWeeks;

            return View(demoOvertimeViewModel);
        }
        
        [HttpPost]
        public ActionResult Index(DemoOvertimeViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            if (!HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).IsDeveloper)
                return RedirectToAction("Index", "Site");
            
            if (!ModelState.IsValid)
                return View(viewModel);
            
            var quarter = _timeService.GetQuarter(new DateTime(2017, 2, 15));
            var quarterViewModel = _viewModelProvider.CreateViewModels(quarter, token, viewModel.BarColumnChartViewModel.IsNormalized, viewModel.InputWeeks);
            quarterViewModel.Quarters = GetSelectList(quarter);
            var demoOvertimeViewModel = _mapper.Map<DemoOvertimeViewModel>(quarterViewModel);
            demoOvertimeViewModel.InputWeeks = viewModel.InputWeeks;

            return View(demoOvertimeViewModel);
        }

        private static IEnumerable<SelectListItem> GetSelectList(Quarter quarter)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Selected = true,
                    Value = quarter.From.ToShortDateString(),
                    Text = quarter.GetDisplayTitle() + " " + quarter.From.Year
                }
            };
        }
    }
}