namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

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
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_not_found);
            }

            if (!this.User.IsAdmin() && submission.IsDeleted)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_not_found);
            }

            if (!this.User.IsAdmin() && this.UserProfile != null && submission.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Submission_not_made_by_user);
            }

            return this.View(submission);
        }

        // TODO: Extract common validations between Download() and Details()
        public FileResult Download(int id)
        {
            var submission = this.Data.Submissions.All()
                .Where(x => x.Id == id)
                .Select(SubmissionDetailsViewModel.FromSubmission)
                .FirstOrDefault();

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_not_found);
            }

            if (!this.User.IsAdmin() && submission.IsDeleted)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Submission_not_found);
            }

            if (!this.User.IsAdmin() && this.UserProfile != null && submission.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Submission_not_made_by_user);
            }

            // TODO: When text content is saved, uncompressing should be performed
            return this.File(submission.Content, "application/octet-stream", string.Format("Submission_{0}.{1}", submission.Id, submission.FileExtension));
        }
    }
}