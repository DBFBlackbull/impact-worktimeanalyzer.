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

            return View(GetVacationViewModel(DateTime.Now, token));
        }

        [HttpPost]
        public ActionResult Index(VacationViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            if (!ModelState.IsValid)
                return View(viewModel);
            
            var dateTime = DateTime.Parse(viewModel.SelectedVacationYear);

            return View(GetVacationViewModel(dateTime, token));
        }
        
        private VacationViewModel GetVacationViewModel(DateTime datetime, SecurityToken token)
        {
            var vacationYear = _timeService.GetVacationYear(datetime);
            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            var vacationDays = _timeRepository.GetVacationDays(vacationYear.StartDate, vacationYear.EndDate, token, profile).ToList();
            vacationDays.AddRange(_holidayService.GetHolidays(vacationYear));

            var vacationViewModel = new VacationViewModel
            {
                DivId = "calendar_chart",
                VacationYears = GetSelectList(vacationYear, profile.HiredDate),
                SelectedVacationYear = vacationYear.StartDate.ToString("s"),
                VacationYear = vacationYear,
                VacationDays = vacationDays,
                SummedVacationDays = Math.Round(vacationDays.Sum(v => v.VacationHours / v.NormalWorkDay), 2).Normalize(),
                SummedExtraVacationDays = Math.Round(vacationDays.Sum(v => v.ExtraVacationHours / v.NormalWorkDay), 2).Normalize(),
                NormalWorkDay = vacationDays.Last().NormalWorkDay
            };
            return vacationViewModel;
        }
        
        private IEnumerable<SelectListItem> GetSelectList(VacationYear selectedYear, DateTime hiredDate)
        {
            var selectListItems = new List<SelectListItem>();

            var currentDate = _timeService.GetVacationYear(hiredDate).StartDate;

            var nowInOneYear = DateTime.Now.AddYears(1);
            while (currentDate < nowInOneYear)
            {
                var currentYear = _timeService.GetVacationYear(currentDate);
                selectListItems.Add(new SelectListItem
                {
                    Selected = currentYear.StartDate == selectedYear.StartDate,
                    Value = currentYear.StartDate.ToString("s"),
                    Text = currentYear.GetShortDisplayString()
                });

                currentDate = currentDate.AddYears(1);
            }
            
            // Special case for mini-vacation period
            // TODO Delete in 5 years when no longer relevant. Lolol like I'm gonna remember this xD
            if (selectListItems.All(v => v.Value != ApplicationConstants.MiniVacationStart.ToString("s")))
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