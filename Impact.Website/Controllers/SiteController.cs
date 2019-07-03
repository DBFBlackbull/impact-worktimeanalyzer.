using System.Web.Mvc;
using Impact.Core.Constants;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.SessionName.FullName] is string fullName))
                return RedirectToAction("Index", "Login");
            
            return View(new SiteViewModel(fullName));
        }
    }
}