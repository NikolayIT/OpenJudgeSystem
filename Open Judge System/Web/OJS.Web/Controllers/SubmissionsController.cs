namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

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

            var submissions = this.GetLastFiftySubmissions();
            return this.View("BasicSubmissions", submissions.ToList());
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest]DataSourceRequest request)
        {
            var data = this.User.IsAdmin() ? this.Data.Submissions.All() : this.Data.Submissions.AllPublic();

            var result = data.Select(SubmissionViewModel.FromSubmission);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            var json = JsonConvert.SerializeObject(result.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }

        // TODO: Extract this method in the submissions repository
        private IEnumerable<SubmissionViewModel> GetLastFiftySubmissions()
        {
            // TODO: add language type
            var submissions = this.Data.Submissions
                .AllPublic()
                .OrderByDescending(x => x.CreatedOn)
                .Take(50)
                .Select(SubmissionViewModel.FromSubmission)
                .ToList();

            return submissions;
        }
    }
}