namespace OJS.Web.Areas.Contests
{
    using System.Web.Mvc;

    using OJS.Common;
    using OJS.Web.Areas.Contests.Controllers;

    public class ContestsAreaRegistration : AreaRegistration
    {
        public override string AreaName => "Contests";

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Contests_list",
                "Contests",
                new { controller = "List", action = GlobalConstants.Index },
                new[] { "OJS.Web.Areas.Contests.Controllers" });

            context.MapRoute(
                "Contests_by_submission_type",
                "Contests/BySubmissionType/{id}/{submissionTypeName}",
                new { controller = "List", action = "BySubmissionType", id = UrlParameter.Optional, submissionTypeName = UrlParameter.Optional });

            context.MapRoute(
                "Contests_by_category",
                "Contests/List/ByCategory/{id}/{category}",
                new { controller = "List", action = "ByCategory", id = UrlParameter.Optional, category = UrlParameter.Optional });

            context.MapRoute(
                "Contests_details",
                "Contests/{id}/{name}",
                new { controller = "Contests", action = "Details", id = UrlParameter.Optional, name = UrlParameter.Optional },
                new { id = "[0-9]+" },
                new[] { "OJS.Web.Areas.Contests.Controllers" });

            context.MapRoute(
               "Contests_results_compete",
               string.Format("Contests/{0}/Results/{{action}}/{{id}}", CompeteController.CompeteActionName),
               new { controller = "Results", action = "Simple", official = true, id = UrlParameter.Optional });

            context.MapRoute(
               "Contests_results_practice",
               string.Format("Contests/{0}/Results/{{action}}/{{id}}", CompeteController.PracticeActionName),
               new { controller = "Results", action = "Simple", official = false, id = UrlParameter.Optional });

            context.MapRoute(
               "Contests_compete",
               string.Format("Contests/{0}/{{action}}/{{id}}", CompeteController.CompeteActionName),
               new { controller = "Compete", action = GlobalConstants.Index, official = true, id = UrlParameter.Optional });

            context.MapRoute(
               "Contests_practice",
               string.Format("Contests/{0}/{{action}}/{{id}}", CompeteController.PracticeActionName),
               new { controller = "Compete", action = GlobalConstants.Index, official = false, id = UrlParameter.Optional });

            context.MapRoute(
               "Contests_default",
               "Contests/{controller}/{action}/{id}",
               new { id = UrlParameter.Optional });
        }
    }
}
