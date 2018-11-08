namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using EntityFramework.Extensions;

    using MissingFeatures;

    using OJS.Common.Helpers;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Business.Submissions.ArchivedSubmissions;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;
    using OJS.Workers.Common.Helpers;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private const string CleanSubmissionsForProcessingTableCronExpression = "0 0 * * *";
        private const string DeleteLeftOverFoldersInTempFolderCronExpression = "0 1 * * *";
        private const string ArchiveOldSubmissionsCronExpression = "30 1 * * MON";

        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly IProblemGroupsDataService problemGroupsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsDataService submissionsData;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            IProblemGroupsDataService problemGroupsData,
            IParticipantScoresDataService participantScoresData,
            ISubmissionsDataService submissionsData)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.problemGroupsData = problemGroupsData;
            this.participantScoresData = participantScoresData;
            this.submissionsData = submissionsData;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                CleanSubmissionsForProcessingTableCronExpression);

            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                DeleteLeftOverFoldersInTempFolderCronExpression);

            return null;
        }

        public ActionResult RegisterJobForArchivingOldSubmissions()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<IArchivedSubmissionsBusinessService>(
                "ArchiveOldSubmissions",
                s => s.ArchiveOldSubmissions(null),
                ArchiveOldSubmissionsCronExpression);

            return null;
        }

        public ActionResult NormalizeParticipantsWithDuplicatedParticipantScores()
        {
            var result = new StringBuilder("<p>Done! Deleted Participant scores:</p><ol>");

            var problemIds = this.Data.Problems.AllWithDeleted().Select(pr => pr.Id).ToList();
            foreach (var problemId in problemIds)
            {
                var participantScoresRepository = new EfGenericRepository<ParticipantScore>(new OjsDbContext());
                var scoresMarkedForDeletion = new List<ParticipantScore>();

                participantScoresRepository
                    .All()
                    .Where(ps => ps.ProblemId == problemId)
                    .GroupBy(p => new { p.ProblemId, p.ParticipantId })
                    .Where(participantScoreGroup => participantScoreGroup.Count() > 1)
                    .ForEach(participantScoreGroup =>
                    {
                        participantScoreGroup
                            .OrderByDescending(ps => ps.Points)
                            .ThenByDescending(ps => ps.Id)
                            .Skip(1)
                            .ForEach(ps => scoresMarkedForDeletion.Add(ps));
                    });

                if (scoresMarkedForDeletion.Any())
                {
                    foreach (var participantScoreForDeletion in scoresMarkedForDeletion)
                    {
                        participantScoresRepository.Delete(participantScoreForDeletion);
                        result.Append($@"<li>ParticipantScore with
                            ParticipantId: {participantScoreForDeletion.ParticipantId} and
                            ProblemId: {participantScoreForDeletion.ProblemId}</li>");
                    }

                    participantScoresRepository.SaveChanges();
                }
            }

            result.Append("</ol>");
            return this.Content(result.ToString());
        }

        public ActionResult DeleteEmptyProblemGroups()
        {
            var softDeleted = this.problemGroupsData
                .GetAll()
                .Where(pg => pg.Problems.All(p => p.IsDeleted))
                .Update(pg => new ProblemGroup
                {
                    IsDeleted = true
                });

            var hardDeleted = this.problemGroupsData
                .GetAllWithDeleted()
                .Where(pg => !pg.Problems.Any())
                .Delete();

            return this.Content($"Done! ProblemGroups set to deleted: {softDeleted}" +
                $"<br/> ProblemGroups hard deleted: {hardDeleted}");
        }

        public ActionResult NormalizeSubmissionAndParticipantScorePoints()
        {
            var updatedSubmissionsCount = 0;
            var updatedParticipantScoresCount = 0;

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                var submissions = this.submissionsData
                    .GetAll()
                    .Include(s => s.Problem)
                    .Where(s => s.Points > s.Problem.MaximumPoints)
                    .ToList();

                foreach (var submission in submissions)
                {
                    submission.Points = submission.Problem.MaximumPoints;

                    this.submissionsData.Update(submission);

                    updatedSubmissionsCount++;
                }

                var participantScores = this.participantScoresData
                    .GetAll()
                    .Include(ps => ps.Problem)
                    .Where(ps => ps.Points > ps.Problem.MaximumPoints)
                    .ToList();

                foreach (var participantScore in participantScores)
                {
                    this.participantScoresData.UpdateBySubmissionAndPoints(
                        participantScore,
                        participantScore.SubmissionId,
                        participantScore.Problem.MaximumPoints);

                    updatedParticipantScoresCount++;
                }

                scope.Complete();
            }

            return this.Content($@"
                <p>Number of updated Submissions: {updatedSubmissionsCount}</p>
                <p>Number of updated Participant Scores: {updatedParticipantScoresCount}</p>");
        }
    }
}