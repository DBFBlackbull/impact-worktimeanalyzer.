using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Constants;
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

        public VacationController(ITimeService timeService, ITimeRepository timeRepository)
        {
            _timeService = timeService;
            _timeRepository = timeRepository;
        }

        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var vacationYear = _timeService.GetVacationYear(DateTime.Now);
            var vacationDays = _timeRepository.GetVacationDays(vacationYear.StartDate, vacationYear.EndDate, token).ToList();

            var vacationViewModel = new VacationViewModel
            {
                DivId = "calendar_chart",
                VacationYears = GetSelectList(vacationYear),
                SelectedVacationYear = vacationYear.StartDate.ToShortDateString(),
                VacationYear = vacationYear,
                VacationDays = vacationDays,
                SummedVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.VacationHours) / 7.5), 2),
                SummedExtraVacationDays = Math.Round(Convert.ToDecimal(vacationDays.Sum(v => v.ExtraVacationHours) / 7.5), 2)
            };

            return View(vacationViewModel);
        }

        [HttpPost]
        public ActionResult Index(VacationViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return View(viewModel);
            
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");
            
            var dateTime = DateTime.Parse(viewModel.SelectedVacationYear);
            var vacationYear = _timeService.GetVacationYear(dateTime);
            var vacationDays = _timeRepository.GetVacationDays(vacationYear.StartDate, vacationYear.EndDate, token).ToList();
            
            var vacationViewModel = new VacationViewModel
            {
                DivId = "calendar_chart",
                VacationYears = GetSelectList(vacationYear),
                SelectedVacationYear = vacationYear.StartDate.ToShortDateString(),
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
                    Value = vacationYear.StartDate.ToShortDateString(),
                    Text = vacationYear.GetShortDisplayString()
                });
            }
            
            // Special case for mini-vacation period
            // Delete in 5 years when no longer relevant. Lolol like I'm gonna remember this xD
            if (selectListItems.All(v => v.Text != "Ferieår 2020/2020"))
            {
                var vacationYear = _timeService.GetVacationYear(new DateTime(2020, 5, 1));
                selectListItems.Add(new SelectListItem
                {
                    Selected = vacationYear.StartDate == selectedYear.StartDate,
                    Value = vacationYear.StartDate.ToShortDateString(),
                    Text = vacationYear.GetShortDisplayString()
                });
            } 

            return selectListItems.OrderBy(v => v.Text).ToList();;
        }
    }
}