using System.Web;
using System.Web.Optimization;

namespace DuckRowNet
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery-basic").Include(
                        "~/Scripts/jquery-1.12.3.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        //"~/Scripts/jquery-{version}.js",
                        "~/Scripts/spin.min.js",
                        //"~/Scripts/jquery-ui-1.8.19.custom.min.js",
                        "~/Scripts/jquery-ui.min.js",
                        "~/Scripts/jquery.fancybox.js",
                        //"~/Scripts/jquery.tools.min.js",
                        "~/Scripts/custom.js",
                        "~/Scripts/rrssb.min.js",
                        "~/Scripts/spin.min.js",
                        "~/Scripts/jquery.flexslider.js"));

            bundles.Add(new ScriptBundle("~/bundles/multistep").Include(
                        "~/Scripts/multistep.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/rrssb.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/jquery.fancybox.css",
                      "~/Content/flexslider.css",
                      "~/Content/site.css"));
        }
    }
}
