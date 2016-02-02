namespace OJS.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using Newtonsoft.Json;

    using OJS.Common;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Web.Common.Attributes;
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
        public ActionResult GetSubmissionsGrid(
            bool notProcessedOnly = false,
            string userId = null,
            int? contestId = null)
        {
            var filter = new SubmissionsFilterViewModel
            {
                ContestId = contestId,
                UserId = userId,
                NotProcessedOnly = this.User.IsAdmin() && notProcessedOnly
            };

            return this.PartialView("_AdvancedSubmissionsGridPartial", filter);
        }

        [HttpPost]
        [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
        public ActionResult ReadSubmissions(
            [DataSourceRequest]DataSourceRequest request,
            string userId,
            bool notProcessedOnly = false,
            int? contestId = null)
        {
            var data = this.Data.Submissions.All();

            if (this.User.IsLecturer() && !this.User.IsAdmin())
            {
                data = data.Where(s =>
                    s.Problem.Contest.Lecturers.Any(lic => lic.LecturerId == this.UserProfile.Id) ||
                    s.Problem.Contest.Category.Lecturers.Any(licc => licc.LecturerId == this.UserProfile.Id));
            }

            if (userId != null)
            {
                data = data.Where(s => s.Participant.UserId == userId);
            }

            // NotProcessedOnly filter is available only for administrators
            if (this.User.IsAdmin() && notProcessedOnly)
            {
                data = data.Where(s => !s.Processed);
            }

            if (contestId.HasValue)
            {
                data = data.Where(s => s.Problem.ContestId == contestId.Value);
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
                        this.TempData.AddInfoMessage("The service was started successfully!");
                    }
                    else
                    {
                        this.TempData.AddDangerMessage("The service is already running or an error has occurred while starting it.");
                    }
                }
            }

            return this.RedirectToAction<SubmissionsController>(c => c.Index());
        }
    }
}