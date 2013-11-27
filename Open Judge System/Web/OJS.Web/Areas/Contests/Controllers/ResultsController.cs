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
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Controllers;

    public class ResultsController : BaseController
    {
        public ResultsController(IOjsData data)
            : base(data)
        {
        }

        /// <summary>
        /// Gets the results for a particular problem for users with at least one submission.
        /// </summary>
        /// <param name="request">The datasource request.</param>
        /// <param name="id">The id of the problem.</param>
        /// <param name="official">A flag checking if the requested results are for practice or for a competition.</param>
        /// <returns>Returns the best result for each user who has at least one submission for the problem.</returns>
        [Authorize]
        public ActionResult ByProblem([DataSourceRequest]DataSourceRequest request, int id, bool official)
        {
            var problem = this.Data.Problems.GetById(id);

            var participant = this.Data.Participants.GetWithContest(problem.ContestId, this.UserProfile.Id, official);

            if (participant == null)
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, "You are not registered for this exam!");
            }

            var results =
                this.Data.Submissions.All()
                    .Where(x => x.ProblemId == problem.Id && x.Participant.IsOfficial == official)
                    .GroupBy(x => x.Participant)
                    .Select(
                        submissionGrouping =>
                        new ProblemResultViewModel
                            {
                                ProblemId = problem.Id,
                                ParticipantName = submissionGrouping.Key.User.UserName,
                                MaximumPoints = problem.MaximumPoints,
                                Result =
                                    submissionGrouping.Where(x => x.ProblemId == problem.Id)
                                    .Max(x => x.Points)
                            });

            return this.Json(results.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Gets the results for a contest.
        /// </summary>
        /// <param name="id">The contest id.</param>
        /// <param name="official">A flag, showing if the results are for practice
        /// or for competition</param>
        /// <returns>Returns a view with the results of the contest.</returns>
        [Authorize]
        public ActionResult Simple(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid contest Id was provided.");
            }

            // if the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                !this.Data.Participants.Any(id, this.UserProfile.Id, official))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, "The results for this contest are not available");
            }

            var contestModel = new ContestResultsViewModel
            {
                Name = contest.Name,
                Problems = contest.Problems.AsQueryable().Select(ContestProblemViewModel.FromProblem).OrderBy(x => x.Name),
                Results = this.Data.Participants.All()
                                                    .Where(x => x.ContestId == contest.Id && x.IsOfficial == official)
                                                    .Select(x => new ParticipantResultViewModel
                                                    {
                                                        ParticipantName = x.User.UserName,
                                                        ProblemResults = x.Contest.Problems
                                                                                    .Select(problem =>
                                                                                                    new ProblemResultPairViewModel
                                                                                                    {
                                                                                                        Id = problem.Id,
                                                                                                        ProblemName = problem.Name,
                                                                                                        Result = problem.Submissions
                                                                                                                            .Where(z => z.ParticipantId == x.Id)
                                                                                                                            .Select(z => z.Points)
                                                                                                                            .OrderByDescending(z => z)
                                                                                                                            .FirstOrDefault()
                                                                                                    })
                                                                                                    .OrderBy(res => res.ProblemName)
                                                    })
                                                    .ToList()
                                                    .OrderByDescending(x => x.Total)
            };

            return this.View(contestModel);
        }

        // TODO: Implement
        // TODO: Unit test
        [Authorize(Roles = "Administrator")]
        public ActionResult Full(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Invalid contest Id was provided.");
            }

            return this.View(contest);
        }
    }
}