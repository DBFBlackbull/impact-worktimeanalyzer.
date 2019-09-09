using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.Website.Models;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class ProfileController : Controller
    {
        // GET
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken))
                return RedirectToAction("Index", "Login");

            var profile = HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile);
            return View(new ProfileViewModel(profile));
        }
    }
}