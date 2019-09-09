using System.Web.Mvc;
using Impact.Business.Login;
using Impact.Core.Constants;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
	public class LoginController : Controller
	{
		private readonly ILoginService _loginService;

		public LoginController(ILoginService loginService)
		{
			_loginService = loginService;
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

			if (!_loginService.IsAuthorized(loginViewModel.Username, loginViewModel.Password, out var token, out var profile))
			{
				loginViewModel.Message = _loginService.FailedLoginMessageHtml();
				return View(loginViewModel);
			}
		    
		    HttpContext.Session.Add(ApplicationConstants.SessionName.Token, token);
		    HttpContext.Session.Add(ApplicationConstants.SessionName.Profile, profile);
		    
		    return RedirectToAction("Index", "Site");
		}

	    public ActionResult Logout()
	    {
	        HttpContext.Session.RemoveAll();
	        return RedirectToAction("Index");
	    }
	}
}