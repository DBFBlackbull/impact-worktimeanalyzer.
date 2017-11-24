using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Core.Contants;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
	public class HomeController : Controller
	{
		private readonly ITimeService _timeService;

		public HomeController(ITimeService timeService)
		{
			_timeService = timeService;
		}

		public ActionResult Index()
		{
			return View(new LoginViewModel());
		}

		[HttpPost]
		public ActionResult Index(LoginViewModel loginViewModel)
		{
		    if (!ModelState.IsValid)
		        return View(loginViewModel);

		    if (!_timeService.IsAuthorized(loginViewModel.Username, loginViewModel.Password, out var token))
		        return View(loginViewModel);
		    
		    HttpContext.Session.Add(ApplicationConstants.Token, token);
		    return RedirectToAction("Index", "Analyzer");
		}

	    public ActionResult Logout()
	    {
	        HttpContext.Session.Remove(ApplicationConstants.Token);
	        return RedirectToAction("Index");
	    }

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}
	}
}