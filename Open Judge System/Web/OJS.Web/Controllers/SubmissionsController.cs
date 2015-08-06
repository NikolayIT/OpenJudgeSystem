namespace OJS.Web.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common.Extensions;
    using OJS.Web.ViewModels.Submission;

    public class SubmissionsController : BaseController
    {
        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.View("AdvancedSubmissions");
            }

            var submissions = this.Data.Submissions
                .GetLastFiftySubmissions()
                .Select(SubmissionViewModel.FromSubmission)
                .ToList();

            return this.View("BasicSubmissions", submissions.ToList());
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest] DataSourceRequest request, string userId)
        {
            IQueryable<Submission> data;

            if (this.User.IsAdmin())
            {
                data = this.Data.Submissions.All();
                if (userId != null)
                {
                    data = data.Where(s => s.Participant.UserId == userId);
                }
            }
            else
            {
                data = this.Data.Submissions.AllPublic();
                if (userId != null)
                {
                    data = data.Where(s => s.Participant.UserId == userId);
                }
            }

            var result = data.Select(SubmissionViewModel.FromSubmission);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string json = JsonConvert.SerializeObject(result.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }
    }
}