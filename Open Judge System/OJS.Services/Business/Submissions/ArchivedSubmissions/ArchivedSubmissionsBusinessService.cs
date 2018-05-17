namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Data.Archives;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private ArchivesDbContext archivesContext;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly string archivesConnectionString;

        public ArchivedSubmissionsBusinessService(
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData,
            string archivesConnectionString)
        {
            this.archivesContext = new ArchivesDbContext();
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
            this.archivesConnectionString = archivesConnectionString;
        }

        public void ArchiveOldSubmissions()
        {
            this.archivesContext.CreateDatabaseIfNotExists(this.archivesConnectionString);

            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveRegularSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.RegularSubmissionEligibleForArchiveAgeInYears);

            const int SubmissionsToTake = 1000;
            var submissionsToSkip = 0;

            IQueryable<ArchivedSubmission> submissionsForArchive;

            do
            {
                submissionsForArchive = this.submissionsData
                    .GetAllWithDeleted()
                    .AsNoTracking()
                    .Where(s => s.CreatedOn < archiveBestSubmissionsLimit ||
                        (s.CreatedOn < archiveRegularSubmissionsLimit &&
                            s.Participant.Scores.All(ps => ps.SubmissionId != s.Id)))
                    .Select(ArchivedSubmission.FromSubmission)
                    .OrderBy(s => s.Id)
                    .Skip(submissionsToSkip)
                    .Take(SubmissionsToTake);

                var submissionIds = new HashSet<int>(submissionsForArchive.Select(s => s.Id));

                using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
                {
                    this.archivesContext.DbContext.Database.Connection.Open();

                    this.archivesContext = AddSubmissionsToArchivesContext(
                        this.archivesContext,
                        submissionsForArchive);

                    this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(submissionIds);
                    this.submissionsData.HardDeleteByIds(submissionIds);

                    scope.Complete();
                }

                submissionsToSkip += SubmissionsToTake;
            } while (submissionsForArchive.Any());
        }

        private static ArchivesDbContext AddSubmissionsToArchivesContext(
            ArchivesDbContext context,
            IEnumerable<ArchivedSubmission> submissions)
        {
            const int CommitRateCount = 100;

            try
            {
                context.DbContext.Configuration.AutoDetectChangesEnabled = false;

                var count = 0;
                foreach (var submission in submissions)
                {
                    context.Submissions.Add(submission);

                    if (++count % CommitRateCount == 0)
                    {
                        context.DbContext.SaveChanges();
                    }
                }

                context.DbContext.SaveChanges();
            }
            finally
            {
                context.DbContext.Dispose();
            }

            return new ArchivesDbContext();
        }
    }
}