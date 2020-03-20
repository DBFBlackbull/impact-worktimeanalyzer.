using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;
using Project = TimeLog.ReportingApi.SDK.Project;

namespace Impact.Website.Controllers
{
    public class JiraIdController : Controller
    {
        private readonly ITimeRepository _timeRepository;
        private readonly string _allProjectIds = Project.All.ToString();

        public JiraIdController(ITimeRepository timeRepository)
        {
            _timeRepository = timeRepository;
        }

        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);

            var viewModel = new JiraIdViewModel
            {
                From = profile.HiredDate,
                To = DateTime.Now.Date,
                Projects = GetSelectList(_allProjectIds, profile.Initials, token)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(JiraIdViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            viewModel.Projects = GetSelectList(viewModel.SelectedProject, profile.Initials, token);
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

        private IEnumerable<SelectListItem> GetSelectList(string selectedProject, string initials, SecurityToken token)
        {
            var selectListItems = new List<SelectListItem>();
            selectListItems.Add(new SelectListItem
            {
                Selected = _allProjectIds == selectedProject, 
                Value = _allProjectIds,
                Text = "Alle Projekter"
            });

            var projects = _timeRepository.GetProjects(initials, token).ToList();
            foreach (var project in projects)
            {
                var projectId = project.Key.ToString();
                selectListItems.Add(new SelectListItem
                {
                    Selected = projectId == selectedProject, 
                    Value = projectId,
                    Text = project.Value
                });
            }
            
            return selectListItems;
        }
    }
}