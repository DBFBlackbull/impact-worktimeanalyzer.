using System.Web.Mvc;
using Impact.Core.Model;
using Impact.Website.Models;

namespace Impact.Website.Controllers
{
    public class DemoOvertimeController : Controller
    {
        // GET
        public ActionResult Index()
        {
            var viewModel = new DemoOvertimeViewModel();
            for (var i = 1; i < 14; i++)
            {
                viewModel.Weeks.Add(new Week
                {
                    Number = i,
                });
            }

            return View(viewModel);
        }
    }
}