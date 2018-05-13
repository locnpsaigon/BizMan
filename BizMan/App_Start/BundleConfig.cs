using System.Web;
using System.Web.Optimization;

namespace BizMan
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.IgnoreList.Clear();
            BundleTable.EnableOptimizations = false;

            // jQuery
            bundles.Add(new ScriptBundle("~/Bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

            // Unobtrusive validation
            bundles.Add(new ScriptBundle("~/Bundles/jqueryval").Include(
                "~/Scripts/jquery.unobtrusive*",
                "~/Scripts/jquery.validate*"));

            // Bootstrap 3.3.5
            bundles.Add(new StyleBundle("~/Content/bootstrap").Include("~/Content/bootstrap.css"));
            bundles.Add(new ScriptBundle("~/Bundles/bootstrap").Include("~/Scripts/bootstrap.min.js"));

            // Bootstrap Dialog 3
            bundles.Add(new StyleBundle("~/Content/bootstrap-dialog").Include("~/Content/bootstrap-dialog.min.css"));
            bundles.Add(new ScriptBundle("~/Bundles/bootstrap-dialog").Include("~/Scripts/bootstrap-dialog.min.js"));

            // Font Awesome 4.4.0
            bundles.Add(new StyleBundle("~/Content/FontAwesome").Include("~/Content/font-awesome.min.css"));
            bundles.Add(new ScriptBundle("~/Bundles/FontAwesome").Include("~/Scripts/font-awesome.min.js"));

            // AdminLTE layout
            bundles.Add(new StyleBundle("~/Content/AdminLTE").Include(
                "~/Content/AdminLTE.css",
                "~/Content/ionicon.min.css",
                "~/Content/Skins/skin-blue.css"));

            bundles.Add(new ScriptBundle("~/Bundles/AdminLTE").Include("~/Scripts/app.js"));

        }
    }
}