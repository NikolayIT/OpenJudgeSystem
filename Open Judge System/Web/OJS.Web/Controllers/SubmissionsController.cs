namespace OJS.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Submission;

    public class SubmissionsController : BaseController
    {
        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (this.User.IsLoggedIn())
            {
                if (this.User.IsAdmin())
                {
                    this.ViewBag.SubmissionsInQueue = this.Data.Submissions.All().Count(x => !x.Processed);
                }

                return this.View("AdvancedSubmissions");
            }

            var submissions = this.Data.Submissions
                .GetLastFiftySubmissions()
                .Select(SubmissionViewModel.FromSubmission)
                .ToList();

            return this.View("BasicSubmissions", submissions);
        }

        [Authorize]
        public ActionResult GetSubmissionsGrid(bool notProcessedOnly = false) =>
            this.PartialView(
                "_AdvancedSubmissionsGridPartial",
                new SubmissionsFilterViewModel { NotProcessedOnly = this.User.IsAdmin() && notProcessedOnly });

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest]DataSourceRequest request, string userId, bool notProcessedOnly = false)
        {
            var data = this.User.IsAdmin() ? this.Data.Submissions.All() : this.Data.Submissions.AllPublic();

            if (userId != null)
            {
                data = data.Where(s => s.Participant.UserId == userId);
            }

            // NotProcessedOnly filter is available only for administrators
            if (this.User.IsAdmin() && notProcessedOnly)
            {
                data = data.Where(s => !s.Processed);
            }

            var result = data.Select(SubmissionViewModel.FromSubmission);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(result.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, GlobalConstants.JsonMimeType);
        }
    }
}