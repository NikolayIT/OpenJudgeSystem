namespace OJS.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;

    using EntityFramework.Extensions;

    using MissingFeatures;

    using OJS.Common.Extensions;
    using OJS.Common.Models;
    using OJS.Data;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Services.Business.Submissions;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ProblemGroups;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Web.Common.Attributes;

    [AuthorizeRoles(SystemRole.Administrator)]
    public class TempController : BaseController
    {
        private const string CleanSubmissionsForProcessingTableCronExpression = "0 0 * * *";
        private const string DeleteLeftOverFoldersInTempFolderCronExpression = "0 1 * * *";
        private const string ArchiveSubmissionsOlderThanOneYearCronExpression = "0 2 * * MON";

        private readonly IHangfireBackgroundJobService backgroundJobs;
        private readonly IProblemGroupsDataService problemGroupsData;

        public TempController(
            IOjsData data,
            IHangfireBackgroundJobService backgroundJobs,
            IProblemGroupsDataService problemGroupsData)
            : base(data)
        {
            this.backgroundJobs = backgroundJobs;
            this.problemGroupsData = problemGroupsData;
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

        public ActionResult RegisterJobForArchivingSubmissionsOlderThanOneYear()
        {
            this.backgroundJobs.AddOrUpdateRecurringJob<ISubmissionsBusinessService>(
                "ArchiveAllSubmissionsOlderThanOneYearExceptBest",
                s => s.ArchiveAllExceptBestOlderThanOneYear(),
                ArchiveSubmissionsOlderThanOneYearCronExpression);

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

        public ActionResult FixMismatchedUserIdFromPlatform()
        {
            const string CorrectUserId = "d921cb09-1c36-4fa6-8ae9-320d1c18eb1e";
            const string RealUserName = "ipetkow";
            const string TempUserName = "tempJudgeUser";

            var tempUserId = this.Data.Users
                .All()
                .Where(u => u.UserName == TempUserName)
                .Select(u => u.Id)
                .SingleOrDefault();

            var realUserId = this.Data.Users
                .All()
                .Where(u => u.UserName == RealUserName)
                .Select(u => u.Id)
                .SingleOrDefault();

            if (realUserId == CorrectUserId)
            {
                return this.Content("The Id is already correct.");
            }

            if (tempUserId == null)
            {
                return this.Content($"No {TempUserName} exists");
            }

            // Attach the Participants of the user to the temp userId
            this.Data.Participants.Update(
                p => p.UserId == realUserId,
                p => new Participant
                {
                    UserId = tempUserId
                });

            // Change the Id of the user
            this.Data.Users.Update(
                u => u.Id == realUserId,
                u => new UserProfile
                {
                    Id = CorrectUserId
                });

            // Reattach the Participants to the correct userId of the user
            this.Data.Participants.Update(
                p => p.UserId == tempUserId,
                p => new Participant
                {
                    UserId = CorrectUserId
                });

            // Hard delete the temp user from the database
            this.Data.Users.Delete(u => u.Id == tempUserId);

            return this.Content("Done.");
        }
    }
}