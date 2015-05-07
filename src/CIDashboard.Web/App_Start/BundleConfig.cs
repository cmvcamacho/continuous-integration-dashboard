using System.Web.Optimization;

namespace CIDashboard.Web
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
              "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
             "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/helpers")
                .Include("~/Scripts/toastr.js")
                .Include("~/Scripts/select.js")
                .Include("~/Scripts/xeditable.js")
                .Include("~/Scripts/jquery.jrumble.1.3.js"));
            
            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/angularjs")
                .Include("~/Scripts/angular.js")
                .Include("~/Scripts/angular-sanitize.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Bootstrap/css").Include(
                        "~/Content/bootstrap.css"));

            bundles.Add(new StyleBundle("~/Helpers/css")
                .Include("~/Content/toastr.css")
                .Include("~/Content/select.css")
                .Include("~/Content/select2.css")
                .Include("~/Content/xeditable.css"));

            bundles.Add(new ScriptBundle("~/bundles/application").Include(
                "~/app/app.js",
                "~/app/services.js",
                "~/app/controllers.js",
                "~/app/filters.js",
                "~/app/services/signalrService.js",
                "~/app/services/projectService.js",
                "~/app/services/editService.js",
                "~/app/controllers/ProjectController.js",
                "~/app/controllers/EditController.js"
                ));
        }
    }
}