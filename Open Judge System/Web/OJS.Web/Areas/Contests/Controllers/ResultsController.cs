namespace OJS.Web.Areas.Contests.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Mvc;

    using Kendo.Mvc.Extensions;
    using Kendo.Mvc.UI;

    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;

    using OJS.Common;
    using OJS.Common.Models;
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
                    .ExistsByContestByUserAndIsOfficial(problem.ProblemGroup.ContestId, this.UserProfile.Id, official))
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
        /// <param name="page">The page on which to open the results table</param>
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
            var userIsParticipant =
                this.participantsData.ExistsByContestByUserAndIsOfficial(id, this.UserProfile.Id, official);

            var resultsAreVisible =
                (official && (contest.ResultsArePubliclyVisible || userIsParticipant)) ||
                (!official && (contest.CanBePracticed || contest.CanBeCompeted)) ||
                this.User.IsAdmin();

            if (!resultsAreVisible)
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var isUserAdminOrLecturerInContest = this.CheckIfUserHasContestPermissions(contest.Id);

            page = page ?? 1;

            var resultsInPage = NotOfficialResultsPageSize;
            if (official)
            {
                resultsInPage = OfficialResultsPageSize;
            }

            var contestResults = this
                .GetContestResults(contest, official, isUserAdminOrLecturerInContest, isFullResults: false)
                .ToPagedResults(page.Value, resultsInPage);

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
            var cacheKey = $"{this.Request.Url?.AbsolutePath}?{nameof(page)}={page}";

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

                contestResults = this
                    .GetContestResults(contest, official, isUserAdminOrLecturerInContest, isFullResults: false)
                    .ToPagedResults(page, resultsInPage);

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

            return this.PartialView("_SimpleResultsPagedList", contestResults);
        }

        // TODO: Unit test
        [Authorize]
        public ActionResult Full(int id, bool official, int? page)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(id);

            page = page ?? 1;

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this
                .GetContestResults(contest, official, isUserAdminOrLecturer: true, isFullResults: true)
                .ToPagedResults(page.Value, NotOfficialResultsPageSize);

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

            var contestResults = this
                .GetContestResults(contest, official, isUserAdminOrLecturer: true, isFullResults: true)
                .ToPagedResults(page, resultsInPage);

            return this.PartialView("_FullResultsPagedList", contestResults);
        }

        [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
        public ActionResult Export(int? id, bool official)
        {
            if (!id.HasValue || !this.CheckIfUserHasContestPermissions(id.Value))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(id.Value);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(
                contest,
                official,
                isUserAdminOrLecturer: true,
                isFullResults: false,
                isExportResults: true);

            // Suggested file name in the "Save as" dialog which will be displayed to the end user
            var fileName = string.Format(
                Resource.Report_excel_format,
                official ? Resource.Contest : Resource.Practice,
                contest.Name);

            return this.ExportResultsToExcel(contestResults, fileName);
        }

        [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
        public ActionResult GetParticipantsAveragePoints(int id)
        {
            if (!this.CheckIfUserHasContestPermissions(id))
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
                    participant.Contest.ProblemGroups
                        .SelectMany(pg => pg.Problems)
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

        [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
        public ActionResult Stats(int contestId, bool official)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
            {
                throw new HttpException((int)HttpStatusCode.Forbidden, Resource.Contest_results_not_available);
            }

            var contest = this.contestsData.GetByIdWithProblems(contestId);

            if (contest == null)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, Resource.Contest_not_found);
            }

            var contestResults = this.GetContestResults(contest, official, true, true);

            var maxResult = this.contestsData.GetMaxPointsById(contest.Id);

            var participantsCount = contestResults.Results.Count();
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

            if (contest.IsOnline && official)
            {
                statsModel.IsGroupedByProblemGroup = true;

                foreach (var problems in contestResults.Problems.GroupBy(p => p.ProblemGroupId))
                {
                    var maxResultsForProblemGroupCount = contestResults.Results
                        .Count(r => r.ProblemResults?
                            .Any(pr => problems
                                .Any(p =>
                                    p.Id == pr.ProblemId &&
                                    pr.BestSubmission.Points == pr.MaximumPoints)) ?? false);

                    var maxPointsForProblemGroup = problems.FirstOrDefault()?.MaximumPoints ?? default(int);
                    var problemGroupName = string.Join("<br/>", problems.Select(p => p.Name));

                    AddStatsByProblem(problemGroupName, maxPointsForProblemGroup, maxResultsForProblemGroupCount);
                    AddStatsByPointsRange(maxPointsForProblemGroup, contestResults.Results);
                }
            }
            else
            {
                foreach (var problem in contestResults.Problems)
                {
                    var maxResultForProblemCount = contestResults.Results
                        .Count(r => r.ProblemResults?
                            .Any(pr =>
                                pr.ProblemId == problem.Id &&
                                pr.BestSubmission.Points == problem.MaximumPoints) ?? false);

                    AddStatsByProblem(problem.Name, problem.MaximumPoints, maxResultForProblemCount);
                    AddStatsByPointsRange(problem.MaximumPoints, contestResults.Results);
                }
            }

            void AddStatsByProblem(string name, int maxPoints, int maxResultsCount)
            {
                var maxResultsPercent = (double)maxResultsCount / participantsCount;

                statsModel.StatsByProblem.Add(new ContestProblemStatsViewModel
                {
                    Name = name,
                    MaxResultsCount = maxResultsCount,
                    MaxResultsPercent = maxResultsPercent,
                    MaxPossiblePoints = maxPoints
                });
            }

            void AddStatsByPointsRange(int maxPoints, IEnumerable<ParticipantResultViewModel> results)
            {
                if (toPoints == 0)
                {
                    toPoints = maxPoints;
                }
                else
                {
                    toPoints += maxPoints;
                }

                // ReSharper disable once AccessToModifiedClosure
                var participantsInPointsRange = results.Count(r => r.Total >= fromPoints && r.Total <= toPoints);

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

        [AuthorizeRoles(SystemRole.Administrator, SystemRole.Lecturer)]
        public ActionResult StatsChart(int contestId)
        {
            if (!this.CheckIfUserHasContestPermissions(contestId))
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
            bool isExportResults = false)
        {
            var contestResults = new ContestResultsViewModel
            {
                Id = contest.Id,
                Name = contest.Name,
                IsCompete = official,
                ContestCanBeCompeted = contest.CanBeCompeted,
                ContestCanBePracticed = contest.CanBePracticed,
                UserHasContestRights = isUserAdminOrLecturer,
                ContestType = contest.Type,
                Problems = contest.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .AsQueryable()
                    .Where(p => !p.IsDeleted)
                    .OrderBy(p => p.OrderBy)
                    .ThenBy(p => p.Name)
                    .Select(ContestProblemListViewModel.FromProblem)
            };

            var participants = this.participantsData
                .GetAllByContestAndIsOfficial(contest.Id, official);

            var participantResults = participants
                .Select(ParticipantResultViewModel.FromParticipantAsSimpleResultByContest(contest.Id))
                .OrderByDescending(parRes => parRes.ProblemResults
                    .Where(pr => pr.ShowResult)
                    .Sum(pr => pr.BestSubmission.Points));

            if (isFullResults)
            {
                participantResults = participants
                    .Select(ParticipantResultViewModel.FromParticipantAsFullResultByContest(contest.Id))
                    .OrderByDescending(parRes => parRes.ProblemResults
                        .Sum(pr => pr.BestSubmission.Points));
            }
            else if (isExportResults)
            {
                participantResults = participants
                    .Select(ParticipantResultViewModel.FromParticipantAsExportResultByContest(contest.Id))
                    .OrderByDescending(parRes => parRes.ProblemResults
                        .Where(pr => pr.ShowResult && !pr.IsExcludedFromHomework)
                        .Sum(pr => pr.BestSubmission.Points));
            }

            contestResults.Results = participantResults
                .ThenBy(parResult => parResult.ProblemResults
                    .OrderByDescending(pr => pr.BestSubmission.Id)
                    .Select(pr => pr.BestSubmission.Id)
                    .FirstOrDefault());

            return contestResults;
        }

        private int CreateResultsSheetHeaderRow(ISheet sheet, ContestResultsViewModel contestResults)
        {
            var headerRow = sheet.CreateRow(0);
            var columnNumber = 0;
            headerRow.CreateCell(columnNumber++).SetCellValue("Username");
            headerRow.CreateCell(columnNumber++).SetCellValue("Name");

            foreach (var problem in contestResults.Problems)
            {
                if (problem.IsExcludedFromHomework)
                {
                    problem.Name = $"(*){problem.Name}";
                }

                headerRow.CreateCell(columnNumber++).SetCellValue(problem.Name);
            }

            var maxPoints = this.contestsData.GetMaxPointsForExportById(contestResults.Id);

            var totalPointsCellTitle = $"Total (Max: {maxPoints})";

            headerRow.CreateCell(columnNumber++).SetCellValue(totalPointsCellTitle);

            return columnNumber;
        }

        private void FillSheetWithParticipantResults(ISheet sheet, ContestResultsViewModel contestResults)
        {
            var rowNumber = 1;
            foreach (var result in contestResults.Results)
            {
                var colNumber = 0;
                var row = sheet.CreateRow(rowNumber++);
                row.CreateCell(colNumber++).SetCellValue(result.ParticipantUsername);
                row.CreateCell(colNumber++).SetCellValue(result.ParticipantFullName);

                foreach (var problem in contestResults.Problems)
                {
                    var problemResult = result.ProblemResults.FirstOrDefault(pr => pr.ProblemId == problem.Id);

                    if (problemResult != null)
                    {
                        row.CreateCell(colNumber++).SetCellValue(problemResult.BestSubmission.Points);
                    }
                    else
                    {
                        row.CreateCell(colNumber++, CellType.Blank);
                    }
                }

                row.CreateCell(colNumber).SetCellValue(result.ExportTotal);
            }
        }

        private FileResult ExportResultsToExcel(ContestResultsViewModel contestResults, string fileName)
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet();

            var columnsCount = this.CreateResultsSheetHeaderRow(sheet, contestResults);

            this.FillSheetWithParticipantResults(sheet, contestResults);

            sheet.AutoSizeColumns(columnsCount);

            // Write the workbook to a memory stream
            var outputStream = new MemoryStream();
            workbook.Write(outputStream);

            // Return the result to the end user
            return this.File(
                outputStream.ToArray(), // The binary data of the XLS file
                GlobalConstants.ExcelMimeType, // MIME type of Excel files
                fileName);
        }
    }
}