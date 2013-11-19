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

    public class ContestsController : BaseController
    {
        public ContestsController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Details(int id)
        {
            var contestViewModel = this.Data.Contests.All()
                                                .Where(x => x.Id == id)
                                                .Select(ContestViewModel.FromContest)
                                                .FirstOrDefault();

            if (contestViewModel == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Could not find a contest with this id.");
            }

            this.ViewBag.ContestProblems = contestViewModel.Problems;
            return this.View(contestViewModel);
        }

        public ActionResult BySubmissionType(int id)
        {
            var contests = this.Data.Contests
                                            .All()
                                            .Where(x => x.SubmissionTypes.Any(s => s.Id == id))
                                            .Select(ContestViewModel.FromContest);

            return this.View(contests);
        }

        [Authorize]
        public ActionResult UserSubmissions([DataSourceRequest]DataSourceRequest request, int contestId)
        {
            var userSubmissions = this.Data.Contests.All()
                                                    .FirstOrDefault(x => x.Id == contestId)
                                                    .Participants
                                                    .Where(x => x.UserId == this.UserProfile.Id)
                                                    .SelectMany(x => x.Submissions
                                                                            .AsQueryable()
                                                                            .Select(SubmissionResultViewModel.FromSubmission));

            return this.Json(userSubmissions.ToDataSourceResult(request));
        }
    }
}
