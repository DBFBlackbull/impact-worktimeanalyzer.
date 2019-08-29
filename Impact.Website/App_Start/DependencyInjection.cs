using System.Web.Mvc;
using AutoMapper;
using Impact.Business.Holiday;
using Impact.Business.Login;
using Impact.Business.Time;
using Impact.DataAccess.Timelog;
using Impact.Website.Models;
using Impact.Website.Providers;
using SimpleInjector;
using SimpleInjector.Integration.Web;
using SimpleInjector.Integration.Web.Mvc;

namespace Impact.Website
{
	public static class DependencyInjection
	{
		public static void SetupDependencyInjection()
		{
			var container = new Container();
			container.Options.DefaultLifestyle = Lifestyle.Transient;
			container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();

			container.Register<ILoginService, TimelogLoginService>(Lifestyle.Scoped);
			container.Register<ITimeService, TimeLogService>(Lifestyle.Scoped);
			container.Register<ITimeRepository, TimeLogRepository>(Lifestyle.Scoped);
			container.Register<OvertimeViewModelProvider, OvertimeViewModelProvider>(Lifestyle.Scoped);
			container.RegisterSingleton<IHolidayService, HolidayService>();
			container.RegisterSingleton(() =>
			{
				var config = new MapperConfiguration(cfg =>
					{
						cfg.CreateMap<QuarterViewModel, DemoOvertimeViewModel>();
					});
				return config.CreateMapper();
			});

			container.Verify();

			DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
		}
	}
}