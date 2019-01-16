using System.Web.Mvc;
using Impact.Core.Contants;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
    public class SiteController : Controller
    {
        // GET: Site
        public ActionResult Index()
        {
            if (!(HttpContext.Session[ApplicationConstants.FullName] is string fullName))
                return RedirectToAction("Index", "Login");
            
            return View(new SiteViewModel(fullName));
        }
    }
}