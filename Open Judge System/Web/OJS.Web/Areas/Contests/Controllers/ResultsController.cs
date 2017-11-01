namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using X.PagedList;

    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;
    
    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ResultsController : BaseController
    {
        public const int OfficialResultsPageSize = 100;
        public const int NotOfficialResultsPageSize = 50;

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

            var results = this.Data.ParticipantScores
                .All()
                .Where(x => x.ProblemId == problem.Id && x.IsOfficial == official)
                .OrderByDescending(x => x.Points)
                .Select(x => new ProblemResultViewModel
                {
                    SubmissionId = x.SubmissionId,
                    ParticipantName = x.ParticipantName,
                    MaximumPoints = problem.MaximumPoints,
                    Result = x.Points
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
            var contest = this.GetContestForSimpleResults(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            // If the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                official &&
                !this.Data.Participants.Any(id, this.UserProfile.Id, official) &&
                !this.User.IsAdmin())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var isUserLecturerInContest =
                this.Data.Users.All().Any(x => x.Id == this.UserProfile.Id && x.LecturerInContests.Any(y => y.ContestId == id));

            var isUserAdminOrLecturerInContest = isUserLecturerInContest || this.User.IsAdmin();

            var cacheKey = this.Request.Url.ToString();

            ContestResultsViewModel contestResults = null;
            if (!official && !isUserAdminOrLecturerInContest)
            {
                contestResults = this.HttpContext.Cache[cacheKey] as ContestResultsViewModel;
            }

            if (contestResults == null)
            {
                var resultsInPage = NotOfficialResultsPageSize;
                if (official)
                {
                    resultsInPage = OfficialResultsPageSize;
                }

                if (page == null || page < 1)
                {
                    page = 1;
                }

                contestResults = this.GetContestResults(
                    contest,
                    official,
                    isUserAdminOrLecturerInContest,
                    page.Value,
                    resultsInPage);

                if (!official && !isUserAdminOrLecturerInContest)
                {
                    this.HttpContext.Cache.Add(
                        key: cacheKey,
                        value: contestResults,
                        dependencies: null,
                        absoluteExpiration: DateTime.Now.AddMinutes(2),
                        slidingExpiration: Cache.NoSlidingExpiration,
                        priority: CacheItemPriority.Normal,
                        onRemoveCallback: null);
                }
            }
            
            contestResults.UserIsLecturerInContest = isUserLecturerInContest;
            this.ViewBag.IsOfficial = official;

            return this.View(contestResults);
        }

        // TODO: Unit test
        [Authorize]
        public ActionResult Full(int id, bool official, int? page)
        {
            if (!this.UserHasAccessToContest(id))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.GetContestForSimpleResults(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            if (page == null || page < 1)
            {
                page = 1;
            }

            var contestResults = this.GetContestFullResults(contest, official, page.Value, NotOfficialResultsPageSize);

            this.ViewBag.IsOfficial = official;

            return this.View(contestResults);
        }

        [Authorize]
        public ActionResult Export(int id, bool official)
        {
            if (!this.UserHasAccessToContest(id))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var model = this.GetContestFullResults(contest, official);

            return this.View(model);
        }

        [Authorize]
        public ActionResult GetParticipantsAveragePoints(int id)
        {
            if (!this.UserHasAccessToContest(id))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contestInfo =
                this.Data.Contests.All()
                    .Where(c => c.Id == id)
                    .Select(
                        c =>
                        new
                        {
                            c.Id,
                            ParticipantsCount = (double)c.Participants.Count(p => p.IsOfficial),
                            c.StartTime,
                            c.EndTime
                        })
                    .FirstOrDefault();

            var submissions = this.Data.Participants.All()
                    .Where(participant => participant.ContestId == contestInfo.Id && participant.IsOfficial)
                    .SelectMany(participant =>
                        participant.Contest.Problems
                            .Where(pr => !pr.IsDeleted)
                            .SelectMany(pr => pr.Submissions
                                .Where(subm => !subm.IsDeleted && subm.ParticipantId == participant.Id)
                                .Select(subm => new
                                {
                                    subm.Points,
                                    subm.CreatedOn,
                                    ParticipantId = participant.Id,
                                    ProblemId = pr.Id
                                })))
                    .OrderBy(subm => subm.CreatedOn)
                    .ToList();

            var viewModel = new List<ContestStatsChartViewModel>();

            for (DateTime time = contestInfo.StartTime.Value.AddMinutes(5); time <= contestInfo.EndTime.Value && time < DateTime.Now; time = time.AddMinutes(5))
            {
                if (!submissions.Any(pr => pr.CreatedOn >= contestInfo.StartTime && pr.CreatedOn <= time))
                {
                    continue;
                }

                var averagePointsLocal = submissions
                    .Where(pr => pr.CreatedOn >= contestInfo.StartTime && pr.CreatedOn <= time)
                    .GroupBy(pr => new { pr.ProblemId, pr.ParticipantId })
                    .Select(gr => new
                    {
                        MaxPoints = gr.Max(pr => pr.Points),
                        gr.Key.ParticipantId
                    })
                    .GroupBy(pr => pr.ParticipantId)
                    .Select(gr => gr.Sum(pr => pr.MaxPoints))
                    .Aggregate((sum, el) => sum + el) / contestInfo.ParticipantsCount;

                viewModel.Add(new ContestStatsChartViewModel
                {
                    AverageResult = averagePointsLocal,
                    Minute = time.Minute,
                    Hour = time.Hour
                });
            }

            return this.Json(viewModel);
        }

        [Authorize]
        public ActionResult Stats(int contestId, bool official)
        {
            if (!this.UserHasAccessToContest(contestId))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.GetContestForSimpleResults(contestId);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestFullResults(contest, official, 1, int.MaxValue);

            var maxResult = contest.Problems.Sum(p => p.MaximumPoints);

            var participantsCount = contestResults.Results.TotalItemCount;
            var statsModel = new ContestStatsViewModel();
            statsModel.MinResultsCount = contestResults.Results.Count(r => r.Total == 0);
            statsModel.MinResultsPercent = (double)statsModel.MinResultsCount / participantsCount;
            statsModel.MaxResultsCount = contestResults.Results.Count(r => r.Total == maxResult);
            statsModel.MaxResultsPercent = (double)statsModel.MaxResultsCount / participantsCount;
            statsModel.AverageResult = (double)contestResults.Results.Sum(r => r.Total) / participantsCount;

            var fromPoints = 0;
            var toPoints = 0;
            foreach (var problem in contestResults.Problems)
            {
                var maxResultsForProblem = contestResults.Results.Count(r => r.ProblemResults.Any(pr => pr.ProblemName == problem.Name && pr.BestSubmission != null && pr.BestSubmission.Points == pr.MaximumPoints));
                var maxResultsForProblemPercent = (double)maxResultsForProblem / participantsCount;
                statsModel.StatsByProblem.Add(new ContestProblemStatsViewModel
                {
                    Name = problem.Name,
                    MaxResultsCount = maxResultsForProblem,
                    MaxResultsPercent = maxResultsForProblemPercent,
                    MaxPossiblePoints = problem.MaximumPoints
                });

                if (toPoints == 0)
                {
                    toPoints = problem.MaximumPoints;
                }
                else
                {
                    toPoints += problem.MaximumPoints;
                }

                var participantsInPointsRange = contestResults.Results.Count(r => r.Total >= fromPoints && r.Total <= toPoints);
                var participantsInPointsRangePercent = (double)participantsInPointsRange / participantsCount;

                statsModel.StatsByPointsRange.Add(new ContestPointsRangeViewModel
                {
                    PointsFrom = fromPoints,
                    PointsTo = toPoints,
                    Participants = participantsInPointsRange,
                    PercentOfAllParticipants = participantsInPointsRangePercent
                });

                fromPoints = toPoints + 1;
            }

            return this.PartialView("_StatsPartial", statsModel);
        }

        [Authorize]
        public ActionResult StatsChart(int contestId)
        {
            if (!this.UserHasAccessToContest(contestId))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            return this.PartialView("_StatsChartPartial", contestId);
        }

        private ContestResultsViewModel GetContestResults(
            Contest contest,
            bool official,
            bool isUserAdminOrLecturer,
            int page,
            int resultsInPage) =>
                new ContestResultsViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    ContestCanBeCompeted = contest.CanBeCompeted,
                    ContestCanBePracticed = contest.CanBePracticed,
                    Problems = this.GetOrderedProblems(contest)
                        .Select(ContestProblemSimpleViewModel.FromProblem),
                    Results = contest.Participants
                        .Where(participant => participant.IsOfficial == official)
                        .Select(participant => new ParticipantResultViewModel
                        {
                            ParticipantUsername = participant.User.UserName,
                            ParticipantFullName =
                                $"{participant.User.UserSettings.FirstName} {participant.User.UserSettings.LastName}".Trim(),
                            ProblemResults = participant.Scores
                                .OrderBy(sc => sc.Problem.OrderBy)
                                .ThenBy(sc => sc.Problem.Name)
                                .Select(score => new ProblemResultPairViewModel
                                {
                                    Id = score.ProblemId,
                                    ShowResult = score.Problem.ShowResults,
                                    BestSubmission = new BestSubmissionViewModel
                                        {
                                            Id = score.SubmissionId,
                                            Points = score.Points
                                        }
                                })
                        })
                        .OrderByDescending(result => isUserAdminOrLecturer ? result.AdminTotal : result.Total)
                        .ThenByDescending(result => result.ProblemResults
                            .OrderBy(pr => pr.BestSubmission.Id)
                            .Select(pr => pr.BestSubmission.Id)
                            .FirstOrDefault())
                        .ToPagedList(page, resultsInPage)
                };

        private ContestFullResultsViewModel GetContestFullResults(
            Contest contest,
            bool official,
            int page = 1,
            int resultsInPage = 0) =>
                new ContestFullResultsViewModel
                    {
                        Id = contest.Id,
                        Name = contest.Name,
                        Problems = this.GetOrderedProblems(contest)
                            .Select(ContestProblemSimpleViewModel.FromProblem),
                        Results = contest.Participants
                            .AsQueryable()
                            .Where(participant => participant.IsOfficial == official)
                            .Select(participant => new ParticipantFullResultViewModel
                            {
                                ParticipantUsername = participant.User.UserName,
                                ParticipantFirstName = participant.User.UserSettings.FirstName,
                                ParticipantLastName = participant.User.UserSettings.LastName,
                                ProblemResults = participant.Contest.Problems
                                    .AsQueryable()
                                    .Where(pr => !pr.IsDeleted)
                                    .OrderBy(pr => pr.OrderBy)
                                    .ThenBy(pr => pr.Name)
                                    .Select(problem => new ProblemFullResultViewModel
                                    {
                                        ProblemName = problem.Name,
                                        MaximumPoints = problem.MaximumPoints,
                                        BestSubmission = problem.ParticipantScores
                                            .AsQueryable()
                                            .Where(ps =>
                                                ps.ParticipantId == participant.Id &&
                                                ps.IsOfficial == official)
                                            .Select(ps => ps.Submission)
                                            .Select(SubmissionFullResultsViewModel.FromSubmission)
                                            .FirstOrDefault()
                                    })
                            })
                            .OrderByDescending(res => res.ProblemResults.Sum(
                                prRes => prRes.BestSubmission == null ? 0 : prRes.BestSubmission.Points))
                            .ThenByDescending(res => res.ProblemResults
                                .OrderByDescending(prRes => prRes.BestSubmission == null ? 0 : prRes.BestSubmission.Id)
                                .Select(prRes => prRes.BestSubmission == null ? 0 : prRes.BestSubmission.Id)
                                .FirstOrDefault())
                            .ToPagedList(page, resultsInPage)
                    };

        private bool UserHasAccessToContest(int contestId) =>
            this.User.IsAdmin() ||
            this.Data.Contests
                .All()
                .Any(c =>
                    c.Id == contestId &&
                    (c.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                        c.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile.Id)));

        private Contest GetContestForSimpleResults(int id)
            => this.Data.Contests
                .All()
                .Include(c => c.Problems)
                .Include(c => c.Participants.Select(par => par.User))
                .Include(c => c.Participants.Select(par => par.Scores.Select(sc => sc.Problem)))
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

        private IOrderedQueryable<Problem> GetOrderedProblems(Contest contest) =>
            contest.Problems
                .AsQueryable()
                .Where(pr => !pr.IsDeleted)
                .OrderBy(pr => pr.OrderBy)
                .ThenBy(pr => pr.Name);
    }
}