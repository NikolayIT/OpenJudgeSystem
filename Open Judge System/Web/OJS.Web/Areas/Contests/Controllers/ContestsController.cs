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
    using OJS.Services.Cache;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Problems;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Problems;
    using OJS.Web.Areas.Contests.ViewModels.Submissions;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ContestsController : BaseController
    {
        private readonly IContestsDataService contestsData;
        private readonly IProblemsDataService problemsData;
        private readonly ICacheItemsProviderService cacheItems;
        private readonly IContestsBusinessService contestsBusiness;

        public ContestsController(
            IOjsData data,
            IContestsDataService contestsData,
            IProblemsDataService problemsData,
            ICacheItemsProviderService cacheItems,
            IContestsBusinessService contestsBusiness)
            : base(data)
        {
            this.contestsData = contestsData;
            this.problemsData = problemsData;
            this.cacheItems = cacheItems;
            this.contestsBusiness = contestsBusiness;
        }

        public ActionResult Details(int id)
        {
            var userId = this.UserProfile?.Id;
            var isUserAdmin = this.User.IsAdmin();

            var contestViewModel = this.contestsData
                .GetByIdQuery(id)
                .Select(ContestViewModel.FromContest)
                .FirstOrDefault();

            var userHasContestRights = isUserAdmin ||
                this.contestsData.IsUserLecturerInByContestAndUser(id, userId);

            if (contestViewModel == null || (!userHasContestRights && !contestViewModel.IsVisible))
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            if (contestViewModel.CategoryId.HasValue)
            {
                contestViewModel.ParentCategories =
                    this.cacheItems.GetContestCategoryParentsList(contestViewModel.CategoryId.Value);
            }

            this.ViewBag.ContestProblems = this.problemsData
                .GetAllByContest(id)
                .Select(ProblemListItemViewModel.FromProblem)
                .ToList();

            contestViewModel.UserIsAdminOrLecturerInContest = userHasContestRights;

            contestViewModel.UserCanCompete = this.contestsBusiness
                .CanUserCompeteByContestByUserAndIsAdmin(contestViewModel.Id, userId, isUserAdmin);

            contestViewModel.UserIsParticipant = this.contestsData
                .IsUserParticipantInByContestAndUser(contestViewModel.Id, userId);

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
                    x.Problem.ProblemGroup.ContestId == contestId &&
                    x.Problem.ShowResults)
                .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }
    }
}
