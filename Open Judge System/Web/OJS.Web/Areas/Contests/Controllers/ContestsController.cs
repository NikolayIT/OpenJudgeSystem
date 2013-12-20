namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ContestsController : BaseController
    {
        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Details(int id)
        {
            var contestViewModel = this.Data.Contests.All()
                                                .Where(x => x.Id == id && !x.IsDeleted && x.IsVisible)
                                                .Select(ContestViewModel.FromContest)
                                                .FirstOrDefault();

            if (contestViewModel == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            this.ViewBag.ContestProblems = this.Data.Problems.All().Where(x => x.ContestId == id)
                .Select(ProblemListItemViewModel.FromProblem);

            return this.View(contestViewModel);
        }

        public ActionResult BySubmissionType(int id)
        {
            var contests = this.Data.Contests
                                            .All()
                                            .Where(x => x.SubmissionTypes.Any(s => s.Id == id) && !x.IsDeleted && x.IsVisible)
                                            .Select(ContestViewModel.FromContest);

            return this.View(contests);
        }

        [Authorize]
        [HttpPost]
        public ActionResult UserSubmissions([DataSourceRequest]DataSourceRequest request, int contestId)
        {
            var userSubmissions = this.Data.Submissions.All()
                                                        .Where(x => 
                                                            x.Participant.UserId == this.UserProfile.Id && 
                                                            x.Problem.ContestId == contestId &&
                                                            x.Problem.ShowResults)
                                                        .Select(SubmissionResultViewModel.FromSubmission);

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }
    }
}
