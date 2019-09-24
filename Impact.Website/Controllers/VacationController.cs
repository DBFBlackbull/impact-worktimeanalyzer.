using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Holiday;
using Impact.Business.Time;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class VacationController : Controller
    {
        private readonly ITimeService _timeService;
        private readonly ITimeRepository _timeRepository;
        private readonly IHolidayService _holidayService;

        public VacationController(ITimeService timeService, ITimeRepository timeRepository, IHolidayService holidayService)
        {
            _timeService = timeService;
            _timeRepository = timeRepository;
            _holidayService = holidayService;
        }

        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var vacationYear = _timeService.GetVacationYear(DateTime.Now);
            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            List<VacationDay> vacationDays = _timeRepository.GetVacationDays(vacationYear.StartDate, vacationYear.EndDate, token, profile).ToList();
            vacationDays.AddRange(_holidayService.GetHolidays(vacationYear));

            var vacationViewModel = new VacationViewModel
            {
                DivId = "calendar_chart",
                VacationYears = GetSelectList(vacationYear),
                SelectedVacationYear = vacationYear.StartDate.ToString("s"),
                VacationYear = vacationYear,
                VacationDays = vacationDays,
                SummedVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.VacationHours) / 7.5), 2),
                SummedExtraVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.ExtraVacationHours) / 7.5), 2),
                NormalWorkDay = profile.NormalWorkDay
            };

            return View(vacationViewModel);
        }

        [HttpPost]
        public ActionResult Index(VacationViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            if (!ModelState.IsValid)
                return View(viewModel);
            
            var dateTime = DateTime.Parse(viewModel.SelectedVacationYear);
            var vacationYear = _timeService.GetVacationYear(dateTime);
            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            var vacationDays = _timeRepository.GetVacationDays(vacationYear.StartDate, vacationYear.EndDate, token, profile).ToList();
            vacationDays.AddRange(_holidayService.GetHolidays(vacationYear));
            
            var vacationViewModel = new VacationViewModel
            {
                DivId = "calendar_chart",
                VacationYears = GetSelectList(vacationYear),
                SelectedVacationYear = vacationYear.StartDate.ToString("s"),
                VacationYear = vacationYear,
                VacationDays = vacationDays,
                SummedVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.VacationHours) / 7.5), 2),
                SummedExtraVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.ExtraVacationHours) / 7.5), 2)
            };

            return View(vacationViewModel);
        }
        
        private IEnumerable<SelectListItem> GetSelectList(VacationYear selectedYear)
        {
            var selectListItems = new List<SelectListItem>();

            var startDate = DateTime.Now.AddYears(1);

            for (int i = 0; i < 6; i++)
            {
                var vacationYear = _timeService.GetVacationYear(startDate.AddYears(i * -1));
                selectListItems.Add(new SelectListItem
                {
                    Selected = vacationYear.StartDate == selectedYear.StartDate,
                    Value = vacationYear.StartDate.ToString("s"),
                    Text = vacationYear.GetShortDisplayString()
                });
            }
            
            // Special case for mini-vacation period
            // TODO Delete in 5 years when no longer relevant. Lolol like I'm gonna remember this xD
            if (selectListItems.All(v => v.Value != ApplicationConstants.MiniVacationStart.ToShortDateString()))
            {
                var vacationYear = _timeService.GetVacationYear(new DateTime(2020, 5, 1));
                selectListItems.Add(new SelectListItem
                {
                    Selected = vacationYear.StartDate == selectedYear.StartDate,
                    Value = vacationYear.StartDate.ToString("s"),
                    Text = vacationYear.GetShortDisplayString()
                });
            } 

            return selectListItems.OrderBy(v => v.Value).ToList();;
        }
    }
}