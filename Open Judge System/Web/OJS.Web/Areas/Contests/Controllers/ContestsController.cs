namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Services.Business.Contests;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Problems;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ContestsController : BaseController
    {
        private readonly IContestsDataService contestsData;
        private readonly IContestsBusinessService contestsBusiness;

        public ContestsController(
            IOjsData data,
            IContestsDataService contestsData,
            IContestsBusinessService contestsBusiness)
            : base(data)
        {
            this.contestsData = contestsData;
            this.contestsBusiness = contestsBusiness;
        }

        public ActionResult Details(int id)
        {
            var isAdmin = this.User.IsAdmin();
            var userId = this.UserProfile?.Id;

            var contestViewModel = this.Data.Contests
                .All()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    (x.IsVisible ||
                        isAdmin ||
                        x.Lecturers.Any(l => l.LecturerId == userId) ||
                        x.Category.Lecturers.Any(cl => cl.LecturerId == userId)))
                .Select(ContestViewModel.FromContest)
                .FirstOrDefault();

            if (contestViewModel == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            this.ViewBag.ContestProblems = this.Data.Problems
                .All()
                .Where(x => x.ContestId == id)
                .Select(ProblemListItemViewModel.FromProblem)
                .ToList();

            contestViewModel.UserIsAdminOrLecturerInContest =
                this.UserProfile != null && this.CheckIfUserHasContestPermissions(id);

            contestViewModel.UserCanCompete = this.UserProfile != null &&
                this.contestsBusiness.CanUserCompeteByContestUserAndIsAdmin(
                    contestViewModel.Id,
                    this.UserProfile.Id,
                    this.User.IsAdmin());

            contestViewModel.IsActive = this.contestsData.IsActiveById(contestViewModel.Id);

            return this.View(contestViewModel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UserSubmissions([DataSourceRequest]DataSourceRequest request, int contestId)
        {
            var userSubmissions = this.Data.Submissions
                .All()
                .Where(x =>
                    x.Participant.UserId == this.UserProfile.Id &&
                    x.Problem.ContestId == contestId &&
                    x.Problem.ShowResults)
                .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }
    }
}
