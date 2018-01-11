using System.Web.Mvc;
using Impact.Business.Holiday;
using Impact.Business.Time;
using Impact.DataAccess.Timelog;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

namespace Impact.Website
{
	public class DependencyInjection
	{
		public static void SetupDependencyInjection()
		{
			var container = new Container();
			container.Options.DefaultLifestyle = Lifestyle.Transient;
			container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

			container.Register<ITimeService, TimeLogService>(Lifestyle.Scoped);
			container.Register<ITimeRepository, TimeLogRepository>(Lifestyle.Scoped);
			container.Register<IHolidayService, HolidayService>(Lifestyle.Scoped);

			container.Verify();

			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
		}
	}
}