namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Transactions;
    using System.Web.Mvc;
    using EntityFramework.Extensions;
    using MissingFeatures;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.Contests;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;
        private readonly IContestsDataService contestsData;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            ISubmissionsForProcessingDataService submissionsForProcessingData,
            IContestsDataService contestsData)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.submissionsForProcessingData = submissionsForProcessingData;
            this.contestsData = contestsData;
        }

        public ActionResult RegisterJobForCleaningSubmissionsForProcessingTable()
        {
            string cron = "0 0 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsForProcessingDataService>(
                "CleanSubmissionsForProcessingTable",
                s => s.Clean(),
                cron);
            
            return null;
        }

        public ActionResult RegisterJobForDeletingLeftOverFilesInTempFolder()
        {
            var cron = "0 1 * * *";
            this.backgroundJobs.AddOrUpdateRecurringJob(
                "DeleteLeftOverFoldersInTempFolder",
                () => DirectoryHelpers.DeleteExecutionStrategyWorkingDirectories(),
                cron);

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

        // TODO: Remove this method after updating the entities
        public void MigrateAllExistingLabsToNewEnumValue() =>
            this.contestsData
                .GetAllWithDeleted()
                .Where(c => c.Type == (ContestType)2)
                .Update(c => new Contest { Type = ContestType.Lab });

        public ActionResult RetestCompileTimeoutSubmissionsFromCSharpDbAdvancedExam10December2017()
        {
            var problemIds = new List<int>() {5477, 5478, 5479, 5480 };
            var submissionIds = this.Data.Context.Submissions
                .Where(s =>
                    !s.IsDeleted &&
                    s.IsCompiledSuccessfully == false &&
                    s.Participant.IsOfficial == true &&
                    s.ProblemId.HasValue &&
                    problemIds.Contains(s.ProblemId.Value))
                .Select(s => s.Id)
                .ToList();

            using (var scope = new TransactionScope())
            {
                this.Data.Context.Submissions
                    .Where(s =>
                        !s.IsDeleted &&
                        s.IsCompiledSuccessfully == false &&
                        s.Participant.IsOfficial == true &&
                        s.ProblemId.HasValue &&
                        problemIds.Contains(s.ProblemId.Value))
                    .Update(x => new Submission { Processed = false });

                this.submissionsForProcessingData.AddOrUpdate(submissionIds);

                scope.Complete();
            }

            return this.Content($"Successfully enqueued {submissionIds.Count()} submissions for retesting.");
        }

        public ActionResult MakeOrderByZeroToAllContestsInProgrammingBasicsExamsCategory()
        {
            const int ProgrammingBasicsExamsCategoryId = 38;

            var categoryName = this.Data.ContestCategories
                .All()
                .Where(cc => cc.Id == ProgrammingBasicsExamsCategoryId)
                .Select(cc => cc.Name)
                .FirstOrDefault();

            if (categoryName == null)
            {
                return this.Content("Category not found");
            }

            var contestsAffected = this.Data.Contests
                .All()
                .Where(c => c.CategoryId == ProgrammingBasicsExamsCategoryId)
                .Update(c => new Contest { OrderBy = 0 });

            return this.Content($"Changed OrderBy of {contestsAffected} contests in the category {categoryName}");
        }
    }
}