using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Impact.Website.Startup))]
namespace Impact.Website
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
