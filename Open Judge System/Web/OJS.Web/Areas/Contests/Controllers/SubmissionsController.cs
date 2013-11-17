namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Net;
    using System.Web;

    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels;
    using System.Web.Mvc;

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
            var submission = this.Data.Submissions.GetById(id);
            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission id was provided!");
            }

            if (!User.IsInRole("Administrator") && submission.Participant != null && this.UserProfile != null && submission.Participant.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This submission is not yours!");
            }

            var model = new SubmissionsDetailsViewModel { Code = submission.ContentAsString };

            return View(model);
        }
	}
}