using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Helper.Comparers;
using Impact.Website.Models;
using Impact.Website.Models.Charts;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using Project = TimeLog.ReportingApi.SDK.Project;

namespace Impact.Website.Controllers
{
    public class PlanningController : Controller
    {
        private readonly ITimeRepository _timeRepository;

        public PlanningController(ITimeRepository timeRepository)
        {
            _timeRepository = timeRepository;
        }

        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);

            var now = DateTime.Now.Date;
            var planningViewModel = GetPlanningViewModel(now.BackTo(DayOfWeek.Monday), now.ForwardTo(DayOfWeek.Sunday), profile, token);
            
            return View(planningViewModel);
        }

        [HttpPost]
        public ActionResult Index(JiraIdViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            if (!ModelState.IsValid)
                return View(viewModel);
            
            ModelState.Clear();
            
            try
            {
                int.TryParse(viewModel.SelectedProject, out var projectId); // failure is the same as all
                viewModel.TimeRegistrations = _timeRepository.GetRegistrationsWithJiraId(viewModel.JiraId, projectId, viewModel.From, viewModel.To, profile, token);
                if (!viewModel.TimeRegistrations.Any())
                    viewModel.Message = $"Kunne ikke finde nogle registreringer med Jira ID '{viewModel.JiraId}'. Prøv et andet Jira ID eller et andet tidsinterval";
                
                return View(viewModel);
            }
            catch (Exception)
            {
                viewModel.Message = "Kunne ikke hente Timelog data da søgningen var for stor. Vælg to datoer tættere på hinanden";
                return View(viewModel);
            }
        }

        private PlanningViewModel GetPlanningViewModel(DateTime from, DateTime to, Profile profile, SecurityToken token)
        {
            var registrationsWithJiraId = _timeRepository.GetRegistrationsWithJiraId(null, Project.All, from, to, profile, token);
            var customerDic = registrationsWithJiraId
                .GroupBy(k => k.CustomerName, v => v)
                .ToDictionary(k => k.Key, v => v.ToList());

            var sortedDictionary = new SortedDictionary<string, List<TimeRegistration>>(customerDic, new CustomerNameComparer());

            var pieData = new List<object[]>
            {
                new object[]
                {
                    new Column {Label = "Hour Type", Type = "string"},
                    new Column {Label = "Hours in period", Type = "number"}
                }
            };

            var random = new Random(123);

            var colors = new List<string>();
            foreach (var (customerName, registrations) in sortedDictionary)
            {
                pieData.Add(new object[] {customerName, registrations.Sum(r => r.RawHours)});
                
                if (ApplicationConstants.CustomerColors.TryGetValue(customerName, out var color))
                    colors.Add(color);
                else
                {
                    var c = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
                    colors.Add($"#{c.R:X2}{c.G:X2}{c.B:X2}");
                }
            }
            
            var optionViewModel = new PieChartViewModel.OptionViewModel
            {
                Title = "Fordeling af arbejdstid på kunder",
                Colors = colors,
                ChartArea = new PieChartViewModel.OptionViewModel.ChartAreaViewModel
                {
                    Left = "0",
                    Width = "90%",
                    Height = "70%"
                }
            };
            
            var pieChartViewModel = new PieChartViewModel
            {
                DivId = "pie_chart",
                Options = optionViewModel,
                RawData = pieData
            };

            return new PlanningViewModel
            {
                From = from,
                To = to,
                PieChartViewModel = pieChartViewModel
            };
        }
    }
}