using System;
using System.Linq;
using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class JiraIdController : Controller
    {
        private readonly ITimeRepository _timeRepository;

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
                To = DateTime.Now.Date
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(JiraIdViewModel viewModel)
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken token))
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
                return View(viewModel);
            
            ModelState.Clear();
            
            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            try
            {
                viewModel.TimeRegistrations = _timeRepository.GetRegistrationsWithJiraId(viewModel.JiraId, viewModel.From, viewModel.To, profile, token);
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
    }
}