namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ResultsController : BaseController
    {
        public const int ResultsPageSize = 300;

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
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.User_is_not_registered_for_exam);
            }

            if (!problem.ShowResults)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Problem_results_not_available);
            }

            var results = this.Data.Submissions
                .All()
                .Where(x => x.ProblemId == problem.Id && x.Participant.IsOfficial == official)
                .GroupBy(x => x.Participant)
                .Select(submissionGrouping => new ProblemResultViewModel
                {
                    ProblemId = problem.Id,
                    ParticipantName = submissionGrouping.Key.User.UserName,
                    MaximumPoints = problem.MaximumPoints,
                    Result = submissionGrouping.Where(x => x.ProblemId == problem.Id).Max(x => x.Points)
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
        public ActionResult Simple(int id, bool official, int? page)
        {
            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            // If the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                !this.Data.Participants.Any(id, this.UserProfile.Id, official) &&
                !this.User.IsAdmin())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var isUserLecturerInContest =
                this.Data.Users.All().Any(x => x.Id == this.UserProfile.Id && x.LecturerInContests.Any(y => y.ContestId == id));

            var isUserAdminOrLecturerInContest = isUserLecturerInContest || this.User.IsAdmin();

            // TODO: Extract choosing the best submission logic to separate repository method?
            var contestResults = this.GetContestResults(contest, official, isUserAdminOrLecturerInContest);

            // calculate page information
            contestResults.TotalResults = contestResults.Results.Count();
            int totalResults = contestResults.TotalResults;
            int totalPages = totalResults % ResultsPageSize == 0 ? totalResults / ResultsPageSize : (totalResults / ResultsPageSize) + 1;

            if (page == null || page < 1)
            {
                page = 1;
            }
            else if (page > totalPages)
            {
                page = totalPages;
            }

            // TODO: optimize if possible
            // query the paged result
            contestResults.Results = contestResults.Results.Skip((page.Value - 1) * ResultsPageSize).Take(ResultsPageSize);

            // add page info to View Model
            contestResults.CurrentPage = page.Value;
            contestResults.AllPages = totalPages;

            contestResults.UserIsLecturerInContest = isUserLecturerInContest;
            this.ViewBag.IsOfficial = official;

            return this.View(contestResults);
        }

        // TODO: Unit test
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public ActionResult Full(int id, bool official)
        {
            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var model = this.GetContestFullResults(contest, official);

            this.ViewBag.IsOfficial = official;

            return this.View(model);
        }

        private ContestResultsViewModel GetContestResults(Contest contest, bool official, bool isUserAdminOrLecturer)
        {
            var contestStartTime = official ? contest.StartTime : contest.PracticeStartTime;

            var contestResults = new ContestResultsViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                ContestCanBeCompeted = contest.CanBeCompeted,
                ContestCanBePracticed = contest.CanBePracticed,
                Problems = contest.Problems
                    .AsQueryable()
                    .Where(x => !x.IsDeleted)
                    .Select(ContestProblemViewModel.FromProblem)
                    .OrderBy(x => x.OrderBy)
                    .ThenBy(x => x.Name),
                Results = this.Data.Participants
                    .All()
                    .Where(participant => participant.ContestId == contest.Id && participant.IsOfficial == official)
                    .Select(participant => new ParticipantResultViewModel
                    {
                        ParticipantUsername = participant.User.UserName,
                        ParticipantFirstName = participant.User.UserSettings.FirstName,
                        ParticipantLastName = participant.User.UserSettings.LastName,
                        ProblemResults = participant.Contest.Problems
                            .Where(x => !x.IsDeleted)
                            .Select(problem => new ProblemResultPairViewModel
                            {
                                Id = problem.Id,
                                ProblemName = problem.Name,
                                ProblemOrderBy = problem.OrderBy,
                                ShowResult = problem.ShowResults,
                                BestSubmission = problem.Submissions
                                    .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                                    .OrderByDescending(z => z.Points)
                                    .ThenByDescending(z => z.Id)
                                    .Select(z => new BestSubmissionViewModel { Id = z.Id, Points = z.Points, CreatedOn = z.CreatedOn })
                                    .FirstOrDefault()
                            })
                            .OrderBy(res => res.ProblemOrderBy)
                            .ThenBy(res => res.ProblemName)
                    })
                    .ToList()
            };

            if (isUserAdminOrLecturer)
            {
                contestResults.Results = contestResults.Results
                    .OrderByDescending(x => x.AdminTotal)
                    .ThenBy(x => x.GetContestTimeInMinutes(contestStartTime));
            }
            else
            {
                contestResults.Results = contestResults.Results
                    .OrderByDescending(x => x.Total)
                    .ThenBy(x => x.GetContestTimeInMinutes(contestStartTime));
            }

            return contestResults;
        }

        private ContestFullResultsViewModel GetContestFullResults(Contest contest, bool official)
        {
            var contestStartTime = official ? contest.StartTime : contest.PracticeStartTime;

            var contestFullResults = new ContestFullResultsViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                Problems = contest.Problems
                    .AsQueryable()
                    .Where(pr => !pr.IsDeleted)
                    .Select(ContestProblemViewModel.FromProblem)
                    .OrderBy(x => x.OrderBy)
                    .ThenBy(x => x.Name),
                Results = this.Data.Participants
                    .All()
                    .Where(participant => participant.ContestId == contest.Id && participant.IsOfficial == official)
                    .Select(participant => new ParticipantFullResultViewModel
                    {
                        ParticipantUsername = participant.User.UserName,
                        ParticipantFirstName = participant.User.UserSettings.FirstName,
                        ParticipantLastName = participant.User.UserSettings.LastName,
                        ProblemResults = participant.Contest.Problems
                            .Where(x => !x.IsDeleted)
                            .Select(problem => new ProblemFullResultViewModel
                            {
                                Id = problem.Id,
                                ProblemName = problem.Name,
                                ProblemOrderBy = problem.OrderBy,
                                MaximumPoints = problem.MaximumPoints,
                                BestSubmission = problem.Submissions
                                    .AsQueryable()
                                    .Where(submission => submission.ParticipantId == participant.Id && !submission.IsDeleted)
                                    .OrderByDescending(z => z.Points)
                                    .ThenByDescending(z => z.Id)
                                    .Select(SubmissionFullResultsViewModel.FromSubmission)
                                    .FirstOrDefault(),
                            })
                            .OrderBy(res => res.ProblemOrderBy)
                            .ThenBy(res => res.ProblemName)
                    })
                    .ToList()
                    .OrderByDescending(x => x.Total)
                    .ThenBy(x => x.GetContestTimeInMinutes(contestStartTime))
            };

            return contestFullResults;
        }
    }
}