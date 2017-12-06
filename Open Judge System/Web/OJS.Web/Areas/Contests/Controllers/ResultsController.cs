namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Collections.Generic;
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
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Attributes;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;
    
    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ResultsController : BaseController
    {
        public const int OfficialResultsPageSize = 100;
        public const int NotOfficialResultsPageSize = 50;

        private readonly IContestsDataService contestsData;
        private readonly IParticipantsDataService participantsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public ResultsController(
            IOjsData data,
            IContestsDataService contestsData,
            IParticipantsDataService participantsData,
            IParticipantScoresDataService participantScoresData)
            : base(data)
        {
            this.contestsData = contestsData;
            this.participantsData = participantsData;
            this.participantScoresData = participantScoresData;
        }

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

            if (!this.participantsData
                    .AnyByContestIdUserIdAndIsOfficial(problem.ContestId, this.UserProfile.Id, official))
            {
                throw new HttpException((int)HttpStatusCode.Unauthorized, Resource.User_is_not_registered_for_exam);
            }

            if (!problem.ShowResults)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Problem_results_not_available);
            }

            var results = this.participantScoresData
                .GetAll()
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
            var contest = this.contestsData.GetByIdWithProblems(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            // If the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                official &&
                !this.participantsData.AnyByContestIdUserIdAndIsOfficial(id, this.UserProfile.Id, official) &&
                !this.User.IsAdmin())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var isUserLecturerInContest =
                this.Data.Users.All().Any(x => x.Id == this.UserProfile.Id && x.LecturerInContests.Any(y => y.ContestId == id));

            var isUserAdminOrLecturerInContest = isUserLecturerInContest || this.User.IsAdmin();

            page = page ?? 1;

            var resultsInPage = NotOfficialResultsPageSize;
            if (official)
            {
                resultsInPage = OfficialResultsPageSize;
            }

            var contestResults = this.GetContestResults(
                contest,
                official,
                isUserAdminOrLecturerInContest,
                isFullResults: false,
                page: page.Value,
                resultsInPage: resultsInPage);

            return this.View(contestResults);
        }

        [AjaxOnly]
        public ActionResult SimplePartial(
            int contestId,
            bool official,
            bool isUserAdminOrLecturerInContest,
            int page,
            int resultsInPage)
        {
            var cacheKey = $"{this.Request.Url.AbsolutePath}?{nameof(page)}={page}";

            ContestResultsViewModel contestResults = null;
            if (!official && !isUserAdminOrLecturerInContest)
            {
                contestResults = this.HttpContext.Cache[cacheKey] as ContestResultsViewModel;
            }

            if (contestResults == null)
            {
                var contest = this.contestsData.GetByIdWithProblems(contestId);

                if (contest == null)
                {
                    throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
                }

                contestResults = this.GetContestResults(
                    contest,
                    official,
                    isUserAdminOrLecturerInContest,
                    isFullResults: false,
                    page: page,
                    resultsInPage: resultsInPage);

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

            return this.PartialView("_SimplePartial", contestResults);
        }

        // TODO: Unit test
        [Authorize]
        public ActionResult Full(int id, bool official, int? page)
        {
            if (!this.contestsData.UserHasAccessByIdUserIdAndIsAdmin(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(id);

            page = page ?? 1;

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(
                contest,
                official,
                isUserAdminOrLecturer: true,
                isFullResults: true,
                page: page.Value,
                resultsInPage: NotOfficialResultsPageSize);

            return this.View(contestResults);
        }

        [AjaxOnly]
        public ActionResult FullPartial(
            int contestId,
            bool official,
            int page,
            int resultsInPage)
        {
            var contest = this.contestsData.GetByIdWithProblems(contestId);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(
                contest,
                official,
                isUserAdminOrLecturer: true,
                isFullResults: true,
                page: page,
                resultsInPage: resultsInPage);

            return this.PartialView("_FullPartial", contestResults);
        }   

        [Authorize]
        public ActionResult Export(int id, bool official)
        {
            if (!this.contestsData.UserHasAccessByIdUserIdAndIsAdmin(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(contest, official, true, true);

            return this.View(contestResults);
        }

        [Authorize]
        public ActionResult GetParticipantsAveragePoints(int id)
        {
            if (!this.contestsData.UserHasAccessByIdUserIdAndIsAdmin(id, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contestInfo = this.contestsData
                    .GetByIdQuery(id)
                    .Select(c => new
                    {
                        c.Id,
                        ParticipantsCount = (double)c.Participants
                            .Where(p => p.Scores.Any())
                            .Count(p => p.IsOfficial),
                        c.StartTime,
                        c.EndTime
                    })
                    .FirstOrDefault();

            var submissions = this.participantsData
                .GetAll()
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
            if (!this.contestsData.UserHasAccessByIdUserIdAndIsAdmin(contestId, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(contestId);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(contest, official, true, true);

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
                            pr.ProblemId == problem.Id &&
                            pr.BestSubmission != null && pr.BestSubmission.Points == problem.MaximumPoints) ?? false);

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
            if (!this.contestsData.UserHasAccessByIdUserIdAndIsAdmin(contestId, this.UserProfile.Id, this.User.IsAdmin()))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            return this.PartialView("_StatsChartPartial", contestId);
        }

        private ContestResultsViewModel GetContestResults(
            Contest contest,
            bool official,
            bool isUserAdminOrLecturer,
            bool isFullResults,
            int page = 1,
            int resultsInPage = int.MaxValue) =>
                new ContestResultsViewModel
                    {
                        Id = contest.Id,
                        Name = contest.Name,
                        IsOfficialResults = official,
                        ContestCanBeCompeted = contest.CanBeCompeted,
                        ContestCanBePracticed = contest.CanBePracticed,
                        UserIsLecturerInContest = isUserAdminOrLecturer,
                        Problems = contest.Problems
                            .AsQueryable()
                            .OrderBy(pr => pr.OrderBy)
                            .ThenBy(pr => pr.Name)
                            .Where(pr => !pr.IsDeleted)
                            .Select(ContestProblemListViewModel.FromProblem),
                        Results = this.GetParticipantResultsAsQueryable(contest.Id, official, isFullResults)
                            .OrderByDescending(parRes => isUserAdminOrLecturer
                                ? parRes.ProblemResults.Sum(pr => pr.BestSubmission.Points)
                                : parRes.ProblemResults.Where(pr => pr.ShowResult).Sum(pr => pr.BestSubmission.Points))
                            .ThenByDescending(parResult => parResult.ProblemResults
                                .OrderBy(prRes => prRes.BestSubmission.Id)
                                .Select(prRes => prRes.BestSubmission.Id)
                                .FirstOrDefault())
                            .ToPagedList(page, resultsInPage)
                    };

        private IQueryable<ParticipantResultViewModel> GetParticipantResultsAsQueryable(
            int contestId,
            bool official,
            bool isFullResults)
        {
            var participantsInContest = this.participantsData
                .GetAllWithScoresByContestIdAndIsOfficialQuery(contestId, official);

            if (isFullResults)
            {
                return participantsInContest.Select(par => new ParticipantResultViewModel
                    {
                        ParticipantUsername = par.User.UserName,
                        ParticipantFirstName = par.User.UserSettings.FirstName,
                        ParticipantLastName = par.User.UserSettings.LastName,
                        ProblemResults = par.Scores
                            .Where(sc => !sc.Problem.IsDeleted && sc.Problem.ContestId == contestId)
                            .Select(sc => new ProblemResultPairViewModel
                            {
                                ProblemId = sc.ProblemId,
                                ShowResult = sc.Problem.ShowResults,
                                MaximumPoints = sc.Problem.MaximumPoints,
                                BestSubmission = new BestSubmissionViewModel
                                {
                                    Id = sc.SubmissionId,
                                    Points = sc.Points,
                                    IsCompiledSuccessfully = sc.Submission.IsCompiledSuccessfully,
                                    SubmissionType = sc.Submission.SubmissionType.Name,
                                    TestRunsCache = sc.Submission.TestRunsCache
                                }
                            })
                    });
            }

            return participantsInContest.Select(par => new ParticipantResultViewModel
                {
                    ParticipantUsername = par.User.UserName,
                    ParticipantFirstName = par.User.UserSettings.FirstName,
                    ParticipantLastName = par.User.UserSettings.LastName,
                    ProblemResults = par.Scores
                        .Where(sc => !sc.Problem.IsDeleted && sc.Problem.ContestId == contestId)
                        .Select(sc => new ProblemResultPairViewModel
                        {
                            ProblemId = sc.ProblemId,
                            ShowResult = sc.Problem.ShowResults,
                            BestSubmission = new BestSubmissionViewModel
                            {
                                Id = sc.SubmissionId,
                                Points = sc.Points
                            }
                        })
                });
        }
    }
}