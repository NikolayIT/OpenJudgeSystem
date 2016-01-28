namespace OJS.Web.Controllers
{
    using System.Diagnostics;
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

        [HttpPost]
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [ValidateAntiForgeryToken]
        public ActionResult StartOjsLocalWorkerService()
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                ErrorDialog = false,
                FileName = "cmd",
                Arguments = "/c NET START \"OJS Local Worker Service\""
            };

            using (var cmdProcess = Process.Start(processStartInfo))
            {
                if (cmdProcess == null)
                {
                    this.TempData.AddDangerMessage("Couldn't start process.");
                }
                else
                {
                    cmdProcess.StartInfo = processStartInfo;
                    cmdProcess.Start();
                    cmdProcess.WaitForExit(GlobalConstants.DefaultProcessExitTimeOutMilliseconds);

                    var error = cmdProcess.StandardError.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(error))
                    {
                        var output = cmdProcess.StandardOutput.ReadToEnd();
                        this.TempData.AddInfoMessage(output);
                    }
                    else
                    {
                        this.TempData.AddDangerMessage(error);
                    }
                }
            }

            return this.RedirectToAction<SubmissionsController>(c => c.Index());
        }
    }
}