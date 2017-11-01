using System.Web.Mvc;
using Impact.Business.Time;
using Impact.Website.Models;
using SimpleInjector;

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

            if (_timeService.IsAuthorized(loginViewModel.Username, loginViewModel.Password))
				return RedirectToAction("Index", "Analyzer");

            return View();
		}

		public ActionResult About()
		{
			ViewBag.Message = "Your application description page.";

			return View();
		}
	}
}