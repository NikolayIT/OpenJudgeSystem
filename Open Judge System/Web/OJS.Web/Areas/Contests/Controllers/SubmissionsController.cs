namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common;
    using OJS.Web.Controllers;

    public class SubmissionsController : BaseController
    {
        public SubmissionsController(IOjsData data)
            : base(data)
        {
        }

        [ActionName("View")]
        [Authorize]
        public ActionResult Details(int id)
        {
            var submission = this.Data.Submissions.All()
                .Where(x => x.Id == id)
                .Select(SubmissionDetailsViewModel.FromSubmission)
                .FirstOrDefault();

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission id was provided!");
            }

            if (!User.IsAdmin() && submission.IsDeleted)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission id was provided!");
            }

            if (!User.IsAdmin() && this.UserProfile != null && submission.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This submission is not yours!");
            }

            return this.View(submission);
        }
    }
}