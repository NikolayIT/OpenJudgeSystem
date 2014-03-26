namespace OJS.Web
{
    using System.Web.Optimization;

    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterScripts(bundles);
            RegisterStyles(bundles);
        }

        private static void RegisterScripts(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/global").Include(
                      "~/Scripts/global.js"));

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                      "~/Scripts/knockout-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                        "~/Scripts/KendoUI/2013.3.1119/kendo.all.js",
                        "~/Scripts/KendoUI/2013.3.1119/kendo.aspnetmvc.js",
                        "~/Scripts/KendoUI/2013.3.1119/cultures/kendo.culture.bg.js",
                        "~/Scripts/KendoUI/2013.3.1119/cultures/kendo.culture.en-GB.js"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror").Include(
                        "~/Scripts/CodeMirror/codemirror.js",
                        "~/Scripts/CodeMirror/mode/clike.js",
                        "~/Scripts/CodeMirror/mode/javascript.js"));
        }

        private static void RegisterStyles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/KendoUI/kendo").Include(
                      "~/Content/KendoUI/kendo.common.css",
                      "~/Content/KendoUI/kendo.blueopal.min.css"));

            bundles.Add(new StyleBundle("~/Content/bootstrap/bootstrap").Include(
                      "~/Content/bootstrap/bootstrap-flatly.css"));

            bundles.Add(new StyleBundle("~/Content/CodeMirror/codemirror").Include(
                      "~/Content/CodeMirror/codemirror.css",
                      "~/Content/CodeMirror/theme/tomorrow-night-eighties.css",
                      "~/Content/CodeMirror/theme/the-matrix.css"));

            bundles.Add(new StyleBundle("~/Content/Contests/submission-page").Include(
                "~/Content/Contests/submission-page.css"));
        }
    }
}
