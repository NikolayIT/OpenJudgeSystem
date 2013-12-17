namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Common;
    using OJS.Web.ViewModels.Submission;

    public class SubmissionsController : BaseController
    {
        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return this.View("AdvancedSubmissions");
            }
            else
            {
                IEnumerable<SubmissionViewModel> submissions = this.GetLastFiftySubmissions();
                return this.View("BasicSubmissions", submissions.ToList());
            }
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest] DataSourceRequest request)
        {
            IQueryable<Submission> data;

            if (User.IsAdmin())
            {
                data = this.Data.Submissions.All();
            }
            else
            {
                data = this.Data.Submissions.AllPublic();
            }

            var result = data.Select(SubmissionViewModel.FromSubmission);

            var serializationSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            string json = JsonConvert.SerializeObject(result.ToDataSourceResult(request), Formatting.None, serializationSettings);
            return this.Content(json, "application/json");
        }

        private IEnumerable<SubmissionViewModel> GetLastFiftySubmissions()
        {
            // TODO: add language type
            var submissions = this.Data.Submissions.AllPublic()
                .OrderByDescending(x => x.CreatedOn)
                .Take(50)
                .Select(SubmissionViewModel.FromSubmission)
                .ToList();

            return submissions;
        }
    }
}