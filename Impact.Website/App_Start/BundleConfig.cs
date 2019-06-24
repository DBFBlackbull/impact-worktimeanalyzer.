using System.Web;
using System.Web.Optimization;

namespace Impact.Website
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			bundles.Add(new ScriptBundle("~/bundles/charts").Include(
						"~/Scripts/Charts/loader.js",
						"~/Scripts/Charts/charts.js"));
			
			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap4").Include(
					  "~/Scripts/Bootstrap/bootstrap.js",
					  "~/Scripts/BootstrapToggle/bootstrap4-toggle.min.js",
					  "~/Scripts/respond.js"));
		    
		    bundles.Add(new StyleBundle("~/Content/site").Include(
		              "~/Content/site.css"));
		    
			bundles.Add(new StyleBundle("~/Content/bootstrap4").Include(
					  "~/Content/Bootstrap/bootstrap.css",
					  "~/Content/BootstrapToggle/bootstrap4-toggle.min.css"));

            bundles.Add(new StyleBundle("~/Content/impact2").Include(
                    "~/Content/Impact/impact.css"));
		}
	}
}
