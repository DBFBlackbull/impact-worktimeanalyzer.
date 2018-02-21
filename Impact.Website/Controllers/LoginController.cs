using System.Web.Mvc;
using Impact.Business.Login;
using Impact.Business.Time;
using Impact.Core.Contants;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
	public class LoginController : Controller
	{
		private readonly ILoginService _loginService;
		private readonly ITimeService _timeService;

		public LoginController(ILoginService loginService, ITimeService timeService)
		{
			_loginService = loginService;
			_timeService = timeService;
		}

		#if !DEBUG
	    [RequireHttps]
	    #endif
		public ActionResult Index()
		{
			return View(new LoginViewModel());
		}

		[HttpPost]
		#if !DEBUG
		[RequireHttps]
		#endif
		public ActionResult Index(LoginViewModel loginViewModel)
		{
		    if (!ModelState.IsValid)
		        return View(loginViewModel);

			if (!_loginService.IsAuthorized(loginViewModel.Username, loginViewModel.Password, out var token))
			{
				loginViewModel.Message = _loginService.FailedLoginMessageHtml();
				return View(loginViewModel);
			}
		    
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