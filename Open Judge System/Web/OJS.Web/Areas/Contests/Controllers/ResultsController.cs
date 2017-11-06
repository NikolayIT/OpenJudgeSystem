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

    using MissingFeatures;

    using X.PagedList;

    using OJS.Data;
    using OJS.Services.Data.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;
    
    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ResultsController : BaseController
    {
        public const int OfficialResultsPageSize = 100;
        public const int NotOfficialResultsPageSize = 50;

        private readonly IContestsDataService contestsData;

        public ResultsController(IOjsData data, IContestsDataService contestsData)
            : base(data) => this.contestsData = contestsData;

        /// <summary>
        /// Gets the results for a particular problem for users with at least one submission.
        /// </summary>
        /// <param name="request">The datasource request.</param>
        /// <param name="id">The id of the problem.</param>
        /// <param name="official">A flag checking if the requested results are for practice or for a competition.</param>
        /// <returns>Returns the best result for each user who has at least one submission for the problem.</returns>
        [Authorize]
        public ActionResult ByProblem([DataSourceRequest] DataSourceRequest request, int id, bool official)
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
                .Where(ps => ps.ProblemId == problem.Id && ps.IsOfficial == official)
                .Select(ps => new ProblemResultViewModel
                {
                    SubmissionId = ps.SubmissionId,
                    ParticipantName = ps.ParticipantName,
                    MaximumPoints = problem.MaximumPoints,
                    Result = ps.Points
                })
                .ToList();

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
            var contest = this.contestsData.GetFirstOrDefault(id);

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

            if (page == null || page < 1)
            {
                page = 1;
                cacheKey += $"?{nameof(page)}={page}";
            }

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

                contestResults = this.GetContestResults(
                    id,
                    official,
                    isFullResults: false,
                    isUserAdminOrLecturer: isUserAdminOrLecturerInContest,
                    page: page.Value,
                    resultsInPage: resultsInPage);

                contestResults.ContestCanBeCompeted = contest.CanBeCompeted;
                contestResults.ContestCanBePracticed = contest.CanBePracticed;

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
            if (!this.contestsData.UserHasAccessToContest(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            if (page == null || page < 1)
            {
                page = 1;
            }

            var contest = this.contestsData.GetFirstOrDefault(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(
                id,
                official,
                isFullResults: true,
                isUserAdminOrLecturer: true,
                page: page.Value,
                resultsInPage: NotOfficialResultsPageSize);

            contestResults.ContestCanBeCompeted = contest.CanBeCompeted;
            contestResults.ContestCanBePracticed = contest.CanBePracticed;

            if (contestResults == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            this.ViewBag.IsOfficial = official;

            return this.View(contestResults);
        }

        [Authorize]
        public ActionResult Export(int id, bool official)
        {
            if (!this.contestsData.UserHasAccessToContest(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contestResults = this.GetContestResults(id, official, true, true);

            if (contestResults == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            return this.View(contestResults);
        }

        [Authorize]
        public ActionResult GetParticipantsAveragePoints(int id)
        {
            if (!this.contestsData.UserHasAccessToContest(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contestInfo = this.contestsData
                    .GetByIdQuery(id)
                    .Select(c => new
                    {
                        c.Id,
                        ParticipantsCount = (double)c.Participants.Count(p => p.IsOfficial),
                        c.StartTime,
                        c.EndTime
                    })
                    .FirstOrDefault();

            var submissions = this.Data.Participants
                .All()
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

            for (var time = contestInfo.StartTime.Value.AddMinutes(5); time <= contestInfo.EndTime.Value && time < DateTime.Now; time = time.AddMinutes(5))
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
            if (!this.contestsData.UserHasAccessToContest(contestId, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contestResults = this.GetContestResults(contestId, official, true, true);

            if (contestResults == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var maxResult = contestResults.Problems.Sum(p => p.MaximumPoints);

            var participantsCount = contestResults.Results.TotalItemCount;
            var statsModel = new ContestStatsViewModel
            {
                MinResultsCount = contestResults.Results.Count(r => r.Total == 0),
                MaxResultsCount = contestResults.Results.Count(r => r.Total == maxResult),
                AverageResult = (double)contestResults.Results.Sum(r => r.Total) / participantsCount
            };

            statsModel.MinResultsPercent = (double)statsModel.MinResultsCount / participantsCount;
            statsModel.MaxResultsPercent = (double)statsModel.MaxResultsCount / participantsCount;

            var fromPoints = 0;
            var toPoints = 0;
            foreach (var problem in contestResults.Problems)
            {
                var maxResultsForProblem = contestResults.Results
                    .Count(r => r.ProblemResults?
                        .Any(pr =>
                            pr.Id == problem.Id &&
                            pr.BestSubmission.Points == problem.MaximumPoints) ?? false);

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
            if (!this.contestsData.UserHasAccessToContest(contestId, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            return this.PartialView("_StatsChartPartial", contestId);
        }

        private ContestResultsViewModel GetContestResults(
            int contestId,
            bool official,
            bool isFullResults,
            bool isUserAdminOrLecturer,
            int page = 1,
            int resultsInPage = int.MaxValue)
        {
            var contestResults = this.contestsData
                .All()
                .Include(c => c.Problems)
                .Where(contest => contest.Id == contestId)
                .Select(contest => new ContestResultsViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    Problems = contest.Problems
                        .AsQueryable()
                        .Where(pr => !pr.IsDeleted)
                        .OrderBy(pr => pr.OrderBy)
                        .ThenBy(pr => pr.Name)
                        .Select(ContestProblemListViewModel.FromProblem)
                })
                .FirstOrDefault();

            if (contestResults == null)
            {
                return null;
            }

            var resultsAsQueryable = this.GetParticipantResults(contestId, isFullResults, official);
            contestResults.Results = resultsAsQueryable.ToPagedList(
                results => isUserAdminOrLecturer ? results.AdminTotal : results.Total,
                page,
                resultsInPage);

            contestResults.Results
                .ForEach(r => r.ParticipantFullName = $"{r.ParticipantFirstName} {r.ParticipantLastName}");

            if (isFullResults)
            {
                contestResults.Results
                    .ForEach(r => r.ProblemResults
                        .ForEach(pr =>
                        {
                            if (pr.BestSubmission != null)
                            {
                                pr.BestSubmission.TestRuns = TestRunFullResultsViewModel.FromCache(
                                    pr.BestSubmission?.TestRunsCache);
                            }
                        }));
            }

            return contestResults;
        }

        private IQueryable<ParticipantResultViewModel> GetParticipantResults(int contestId, bool isFullResults, bool official)
        {
            if (isFullResults)
            {
                return this.Data.Participants
                    .All()
                    .Include(par => par.User)
                    .Include(par => par.Contest.Problems
                        .Select(pr => pr.ParticipantScores
                            .Select(sc => sc.Submission)))
                    .AsNoTracking()
                    .Where(p => p.IsOfficial == official && p.ContestId == contestId)
                    .Select(par => new ParticipantResultViewModel
                    {
                        ParticipantUsername = par.User.UserName,
                        ParticipantFirstName = par.User.UserSettings.FirstName,
                        ParticipantLastName = par.User.UserSettings.LastName,
                        ProblemResults = par.Contest.Problems
                            .Where(pr => !pr.IsDeleted)
                            .OrderBy(pr => pr.OrderBy)
                            .ThenBy(pr => pr.Name)
                            .Select(pr => new ProblemResultPairViewModel
                            {
                                Id = pr.Id,
                                MaximumPoints = pr.MaximumPoints,
                                ShowResult = pr.ShowResults,
                                BestSubmission = pr.ParticipantScores
                                    .Where(parScore =>
                                        parScore.ParticipantId == par.Id &&
                                        parScore.IsOfficial == official)
                                    .Select(sc => new BestSubmissionViewModel
                                    {
                                        Points = sc.Points,
                                        Id = sc.SubmissionId,
                                        IsCompiledSuccessfully = sc.Submission.IsCompiledSuccessfully,
                                        SubmissionType = sc.Submission.SubmissionType.Name,
                                        TestRunsCache = sc.Submission.TestRunsCache
                                    })
                                    .FirstOrDefault()
                            })
                    });
            }

            return this.Data.Participants
                .All()
                .Include(par => par.User)
                .Include(par => par.Contest.Problems.Select(pr => pr.ParticipantScores))
                .AsNoTracking()
                .Where(p => p.IsOfficial == official && p.ContestId == contestId)
                .Select(par => new ParticipantResultViewModel
                {
                    ParticipantUsername = par.User.UserName,
                    ParticipantFirstName = par.User.UserSettings.FirstName,
                    ParticipantLastName = par.User.UserSettings.LastName,
                    ProblemResults = par.Contest.Problems
                        .Where(pr => !pr.IsDeleted)
                        .OrderBy(pr => pr.OrderBy)
                        .ThenBy(pr => pr.Name)
                        .Select(pr => new ProblemResultPairViewModel
                        {
                            Id = pr.Id,
                            ShowResult = pr.ShowResults,
                            BestSubmission = pr.ParticipantScores
                                .Where(parScore =>
                                    parScore.ParticipantId == par.Id &&
                                    parScore.IsOfficial == official)
                                .Select(parScore => new BestSubmissionViewModel
                                {
                                    Id = parScore.SubmissionId,
                                    Points = parScore.Points
                                })
                                .FirstOrDefault()
                        })
                });
        }
    }
}