namespace OJS.Web.Controllers
{
    using Kendo.Mvc.UI;
    using Kendo.Mvc.Extensions;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using OJS.Data;
    using OJS.Web.ViewModels.Submission;
    using OJS.Web.ViewModels.TestRun;
    using OJS.Data.Models;

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
                return View("AdvancedSubmissions");
            }
            else
            {
                IEnumerable<SubmissionViewModel> submissions = GetLastFiftySubmissions();
                return View("BasicSubmissions", submissions);
            }
        }

        [HttpPost]
        public ActionResult ReadSubmissions([DataSourceRequest] DataSourceRequest request)
        {
            IQueryable<Submission> data;

            if (User.IsInRole("Administrator"))
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