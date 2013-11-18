namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;

    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
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
                .Select(x => new SubmissionDetailsViewModel
                {
                    Id = x.Id,
                    UserId = x.Participant.UserId,
                    UserName = x.Participant.User.UserName,
                    CompilerComment = x.CompilerComment,
                    Content = x.Content,
                    CreatedOn = x.CreatedOn,
                    IsCompiledSuccessfully = x.IsCompiledSuccessfully,
                    IsDeleted = x.IsDeleted,
                    Points = x.Points,
                    Processed = x.Processed,
                    Processing = x.Processing,
                    ProblemId = x.ProblemId,
                    ProblemName = x.Problem.Name,
                    ProcessingComment = x.ProcessingComment,
                    SubmissionType = x.SubmissionType,
                    TestRuns = x.TestRuns.Select(y => new TestRunDetailsViewModel
                                                          {
                                                              IsTrialTest = y.Test.IsTrialTest,
                                                              CheckerComment = y.CheckerComment,
                                                              ExecutionComment = y.ExecutionComment,
                                                              Order = y.Test.OrderBy,
                                                              ResultType = y.ResultType,
                                                              TimeUsed = y.TimeUsed,
                                                              MemoryUsed = y.MemoryUsed,
                                                              Id = y.Id
                                                          }),
                })
                .FirstOrDefault();

            if (submission == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission id was provided!");
            }

            if (!User.IsInRole("Administrator") && submission.IsDeleted)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid submission id was provided!");
            }

            if (!User.IsInRole("Administrator") && this.UserProfile != null && submission.UserId != this.UserProfile.Id)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "This submission is not yours!");
            }

            return View(submission);
        }
	}
}