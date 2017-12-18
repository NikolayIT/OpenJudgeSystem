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
    using Microsoft.Ajax.Utilities;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;

    public class ResultsController : BaseController
    {
        public const int OfficialResultsPageSize = 100;
        public const int NotOfficialResultsPageSize = 50;

        private readonly IParticipantScoresDataService participantScoresData;

        public ResultsController(IOjsData data, IParticipantScoresDataService participantScoresData)
            : base(data) =>
                this.participantScoresData = participantScoresData;

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
            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == id);

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
                contestResults = this.GetContestResults(contest, official, isUserAdminOrLecturerInContest);

                var resultsInPage = NotOfficialResultsPageSize;
                if (official)
                {
                    resultsInPage = OfficialResultsPageSize;
                }

                if (page == null || page < 1)
                {
                    page = 1;
                }
                
                // query the paged result
                contestResults.Results = contestResults
                    .Results
                    .Skip((page.Value - 1) * resultsInPage)
                    .Take(resultsInPage)
                    .ToArray();

                contestResults.Results.ForEach(x => x.ProblemResults.ForEach(w => w.IsPartOfUserProblems = x.ParticipantProblemIds.Contains(w.Id)));

                // add page info to View Model
                contestResults.CurrentPage = page.Value;

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

            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestFullResults(contest, official);

            if (page == null || page < 1)
            {
                page = 1;
            }

            var resultsInPage = NotOfficialResultsPageSize;

            // query the paged result
            contestResults.Results = contestResults
                .Results
                .Skip((page.Value - 1) * resultsInPage)
                .Take(resultsInPage)
                .ToArray();

            // add page info to View Model
            contestResults.CurrentPage = page.Value;

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
            
            var contest = this.Data.Contests.All().Include(x => x.Problems).FirstOrDefault(x => x.Id == contestId);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestFullResults(contest, official);

            var maxResult = this.Data.Contests.All().FirstOrDefault(c => c.Id == contestResults.Id).Problems.Sum(p => p.MaximumPoints);
            var participantsCount = contestResults.Results.Count();
            var statsModel = new ContestStatsViewModel();
            statsModel.MinResultsCount = contestResults.Results.Count(r => r.Total == 0);
            statsModel.MinResultsPercent = (double)statsModel.MinResultsCount / participantsCount;
            statsModel.MaxResultsCount = contestResults.Results.Count(r => r.Total == maxResult);
            statsModel.MaxResultsPercent = (double)statsModel.MaxResultsCount / participantsCount;
            statsModel.AverageResult = (double)contestResults.Results.Sum(r => r.Total) / participantsCount;

            int fromPoints = 0;
            int toPoints = 0;
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

        private ContestResultsViewModel GetContestResults(Contest contest, bool official, bool isUserAdminOrLecturer)
        {
            var contestResults = new ContestResultsViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                ContestType = contest.Type,
                IsCompete = official,
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
                        ParticipantProblemIds = participant.Problems.Select(p => p.Id),
                        ProblemResults = participant.Contest.Problems
                            .Where(x => !x.IsDeleted)
                            .Select(problem => new ProblemResultPairViewModel
                            {
                                Id = problem.Id,
                                ProblemName = problem.Name,
                                ProblemOrderBy = problem.OrderBy,
                                ShowResult = problem.ShowResults,
                                BestSubmission = problem.ParticipantScores
                                    .Where(z => z.ParticipantId == participant.Id && z.IsOfficial == official)
                                    .Select(z => new BestSubmissionViewModel { Id = z.SubmissionId, Points = z.Points })
                                    .FirstOrDefault()
                            })
                            .OrderBy(res => res.ProblemOrderBy)
                            .ThenBy(res => res.ProblemName)
                    })
            };

            IOrderedEnumerable<ParticipantResultViewModel> result;

            if (isUserAdminOrLecturer)
            {
                result = contestResults.Results
                    .OrderByDescending(x => x.AdminTotal);
            }
            else
            {
                result = contestResults.Results
                    .OrderByDescending(x => x.Total);
            }

            contestResults.Results = result
                .ThenByDescending(x => x.ProblemResults
                    .OrderBy(y => y.BestSubmission?.Id)
                    .Select(y => y.BestSubmission?.Id)
                    .FirstOrDefault());

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
                                ProblemName = problem.Name,
                                ProblemOrderBy = problem.OrderBy,
                                MaximumPoints = problem.MaximumPoints,
                                BestSubmission = problem.ParticipantScores
                                    .AsQueryable()
                                    .Where(z => z.ParticipantId == participant.Id && z.IsOfficial == official)
                                    .Select(z => z.Submission)
                                    .Select(SubmissionFullResultsViewModel.FromSubmission)
                                    .FirstOrDefault(),
                            })
                            .OrderBy(res => res.ProblemOrderBy)
                            .ThenBy(res => res.ProblemName)
                    })
            };

            contestFullResults.Results = contestFullResults.Results
                .OrderByDescending(x => x.ProblemResults.Sum(z => z.BestSubmission?.Points ?? 0))
                .ThenByDescending(x => x.ProblemResults
                    .OrderByDescending(y => y.BestSubmission?.Id)
                    .Select(y => y.BestSubmission?.Id)
                    .FirstOrDefault());

            return contestFullResults;
        }

        private bool UserHasAccessToContest(int contestId) =>
            this.User.IsAdmin() ||
            this.Data.Contests
                .All()
                .Any(c =>
                    c.Id == contestId &&
                    (c.Lecturers.Any(l => l.LecturerId == this.UserProfile.Id) ||
                        c.Category.Lecturers.Any(cl => cl.LecturerId == this.UserProfile.Id)));
    }
}