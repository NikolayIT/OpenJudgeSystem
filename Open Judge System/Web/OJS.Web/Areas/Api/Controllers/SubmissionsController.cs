namespace OJS.Web.Areas.Api.Controllers
{
    using System.Linq;
    using System.Web.Mvc;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Api.Models;
    using OJS.Web.Common;

    public class SubmissionsController : Controller
    {
        public SubmissionsController(IOjsData data)
        {
            this.Data = data;
        }

        protected IOjsData Data { get; set; }

        public ActionResult GetContestantBestSubmissions(string apiKey, string userId, int? contestId)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(userId) || !contestId.HasValue)
            {
                return this.Json(new ErrorMessageViewModel("Invalid arguments!"), JsonRequestBehavior.AllowGet);
            }

            var userManager = new OjsUserManager<UserProfile>(new UserStore<UserProfile>(this.Data.Context.DbContext));

            var userIsAdmin = userManager.IsInRole(userId, GlobalConstants.AdministratorRoleName);
            var isValidApiKey = this.Data.Users
                .All()
                .Any(x => x.Id == apiKey);
            if (!isValidApiKey && userIsAdmin)
            {
                return this.Json(new ErrorMessageViewModel("Invalid API key!"), JsonRequestBehavior.AllowGet);
            }

            var submissions = this.Data.Submissions
                .All()
                .Where(x => x.Participant.IsOfficial && x.Participant.UserId == userId && x.Participant.ContestId == contestId.Value)
                .GroupBy(x => x.Problem)
                .Select(x => new
                {
                    Problem = x.Key.Name,
                    Submission = x.OrderByDescending(y => y.Points)
                        .ThenByDescending(y => y.CreatedOn)
                        .Select(y => new { y.Content, y.FileExtension })
                        .FirstOrDefault()
                })
                .ToList();

            return this.Json(submissions, JsonRequestBehavior.AllowGet);
        }
    }
}