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
                        "~/Dependencies/jquery/jquery.js",
                        "~/Dependencies/jquery-ajax-unobtrusive/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Dependencies/jquery-validation/jquery.validate.js",
                        "~/Dependencies/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Dependencies/bootstrap/js/bootstrap.js"));

            bundles.Add(new ScriptBundle("~/bundles/knockout").Include(
                      "~/Scripts/knockout-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                        "~/Scripts/KendoUI/2014.3.1411/kendo.all.js",
                        "~/Scripts/KendoUI/2014.3.1411/kendo.aspnetmvc.js",
                        "~/Scripts/KendoUI/2014.3.1411/cultures/kendo.culture.bg.js",
                        "~/Scripts/KendoUI/2014.3.1411/cultures/kendo.culture.en-GB.js"));

            bundles.Add(new ScriptBundle("~/bundles/codemirror").Include(
                        "~/Scripts/CodeMirror/codemirror.js",
                        "~/Scripts/CodeMirror/mode/clike.js",
                        "~/Scripts/CodeMirror/mode/javascript.js"));

            bundles.Add(new ScriptBundle("~/bundles/codemirrormerge").Include(
                        "~/Scripts/CodeMirror/addon/diff_match_patch.js",
                        "~/Scripts/CodeMirror/addon/merge.js"));
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

            bundles.Add(new StyleBundle("~/Content/CodeMirror/codemirrormerge").Include(
                      "~/Content/CodeMirror/addon/merge.css",
                      "~/Content/Contests/submission-view-page.css"));

            bundles.Add(new StyleBundle("~/Content/Contests/submission-page").Include(
                      "~/Content/Contests/submission-page.css"));

            bundles.Add(new StyleBundle("~/Content/CodeMirror/codemirrormerge").Include(
                      "~/Content/CodeMirror/addon/merge.css",
                      "~/Content/Contests/submission-view-page.css"));
        }
    }
}
