namespace OJS.Web.Areas.Contests.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using OJS.Common;
    using OJS.Data;
    using OJS.Web.Areas.Contests.ViewModels.Contests;
    using OJS.Web.Areas.Contests.ViewModels.Results;
    using OJS.Web.Common.Extensions;
    using OJS.Web.Controllers;

    using Resource = Resources.Areas.Contests.ContestsGeneral;
    using System;
    using System.Collections.Generic;
    
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
        public ActionResult Simple(int id, bool official, int? page)
        {
            var contest = this.Data.Contests.GetById(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            // if the results are not visible and the participant is not registered for the contest
            // then he is not authorized to view the results
            if (!contest.ResultsArePubliclyVisible &&
                !this.Data.Participants.Any(id, this.UserProfile.Id, official) &&
                !this.User.IsAdmin())
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            // TODO: Extract choosing the best submission logic to separate repository method?
            var contestModel = new ContestResultsViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                ContestCanBeCompeted = contest.CanBeCompeted,
                ContestCanBePracticed = contest.CanBePracticed,
                Problems = contest.Problems.AsQueryable().Where(x => !x.IsDeleted)
                                        .Select(ContestProblemViewModel.FromProblem).OrderBy(x => x.OrderBy).ThenBy(x => x.Name),
                Results = this.Data.Participants.All()
                    .Where(participant => participant.ContestId == contest.Id && participant.IsOfficial == official)
                    .Select(participant => new ParticipantResultViewModel
                    {
                        ParticipantUsername = participant.User.UserName,
                        ParticipantFirstName = participant.User.UserSettings.FirstName,
                        ParticipantLastName = participant.User.UserSettings.LastName, 
                        ProblemResults = participant.Contest.Problems
                            .Where(x => !x.IsDeleted)
                            .Select(problem =>
                                new ProblemResultPairViewModel
                                {
                                    Id = problem.Id,
                                    ProblemName = problem.Name,
                                    ProblemOrderBy = problem.OrderBy,
                                    ShowResult = problem.ShowResults,
                                    BestSubmission = problem.Submissions
                                                        .Where(z => z.ParticipantId == participant.Id && !z.IsDeleted)
                                                        .OrderByDescending(z => z.Points).ThenByDescending(z => z.Id)
                                                        .Select(z => new BestSubmissionViewModel { Id = z.Id, Points = z.Points })
                                                        .FirstOrDefault()
                                })
                                .OrderBy(res => res.ProblemOrderBy).ThenBy(res => res.ProblemName)
                    })
                    .ToList()
                    .OrderByDescending(x => x.Total)
            };

            // calculate page information
            contestModel.TotalResults = contestModel.Results.Count();
            int totalResults = contestModel.TotalResults;
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
            contestModel.Results = contestModel.Results
                    .Skip((page.Value - 1) * ResultsPageSize)
                    .Take(ResultsPageSize);
            
            // add page info to View Model
            contestModel.CurrentPage = page.Value;
            contestModel.AllPages = totalPages;

            if (User.IsAdmin())
            {
                contestModel.Results = contestModel.Results.OrderByDescending(x => x.AdminTotal);
            }

            this.ViewBag.IsOfficial = official;

            return this.View(contestModel);
        }

        // TODO: Unit test
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public ActionResult Full(int id, bool official)
        {
            var contest = this.Data.Contests.GetById(id);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var model = new ContestFullResultsViewModel
                {
                    Id = contest.Id,
                    Name = contest.Name,
                    Problems = contest.Problems.AsQueryable().Where(pr => !pr.IsDeleted).Select(ContestProblemViewModel.FromProblem).OrderBy(x => x.OrderBy).ThenBy(x => x.Name),
                    Results = this.Data.Participants.All()
                        .Where(participant => participant.ContestId == contest.Id && participant.IsOfficial == official)
                        .Select(participant => new ParticipantFullResultViewModel
                        {
                            ParticipantUsername = participant.User.UserName,
                            ParticipantFirstName = participant.User.UserSettings.FirstName,
                            ParticipantLastName = participant.User.UserSettings.LastName, 
                            ProblemResults = participant.Contest.Problems
                                .Where(x => !x.IsDeleted)
                                .Select(problem =>
                                    new ProblemFullResultViewModel
                                    {
                                        Id = problem.Id,
                                        ProblemName = problem.Name,
                                        ProblemOrderBy = problem.OrderBy,
                                        MaximumPoints = problem.MaximumPoints,
                                        BestSubmission = problem.Submissions.AsQueryable()
                                                            .Where(submission => submission.ParticipantId == participant.Id && !submission.IsDeleted)
                                                            .OrderByDescending(z => z.Points).ThenByDescending(z => z.Id)
                                                            .Select(SubmissionFullResultsViewModel.FromSubmission)
                                                            .FirstOrDefault(),
                                    })
                                    .OrderBy(res => res.ProblemOrderBy).ThenBy(res => res.ProblemName)
                        })
                        .ToList()
                        .OrderByDescending(x => x.Total).ThenBy(x => x.ParticipantUsername)
                };

            this.ViewBag.IsOfficial = official;

            return this.View(model);
        }
        
        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public ActionResult Stats(ContestFullResultsViewModel viewModel)
        {
            var maxResult = this.Data.Contests.All().Where(c => c.Id == viewModel.Id).FirstOrDefault().Problems.Sum(p => p.MaximumPoints);
            var participantsCount = viewModel.Results.Count();
            var statsModel = new ContestStatsViewModel();
            statsModel.MinResultsCount = viewModel.Results.Count(r => r.Total == 0);
            statsModel.MinResultsPercent = (double)statsModel.MinResultsCount / participantsCount;
            statsModel.MaxResultsCount = viewModel.Results.Count(r => r.Total == maxResult);
            statsModel.MaxResultsPercent = (double)statsModel.MaxResultsCount / participantsCount;
            statsModel.AverageResult = (double)viewModel.Results.Sum(r => r.Total) / participantsCount;

            int fromPoints = 0;
            int toPoints = 0;
            foreach (var problem in viewModel.Problems)
	        {                
                var maxResultsForProblem = viewModel.Results.Count(r => r.ProblemResults.Any(pr => pr.ProblemName == problem.Name && pr.BestSubmission != null && pr.BestSubmission.Points == pr.MaximumPoints));
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

                var participantsInPointsRange = viewModel.Results.Count(r => r.Total >= fromPoints && r.Total <= toPoints);
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
    }
}