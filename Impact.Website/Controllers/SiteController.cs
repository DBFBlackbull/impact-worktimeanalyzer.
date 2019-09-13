using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Core.Extension;
using Impact.Core.Model;
using Impact.Website.Models;
using TimeLog.TransactionalApi.SDK.ProjectManagementService;

namespace Impact.Website.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.Token] is SecurityToken))
                return RedirectToAction("Index", "Login");
            
            return View(new SiteViewModel(HttpContext.Session.Get<Profile>(ApplicationConstants.SessionName.Profile).FullName));
        }
        
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Racer()
        {
            return View();
        }
    }
}