namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;

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

            var oneYearLimit = DateTime.Now.AddYears(-1);
            var twoYearsLimit = DateTime.Now.AddYears(-2);
            const int CommitRateCount = 100;
            const int SubmissionsToTake = 1000;
            var submissionsToSkip = 0;

            IQueryable<ArchivedSubmission> submissionsForArchive;

            do
            {
                this.archivesContext = new ArchivesDbContext();

                submissionsForArchive = this.submissionsData
                    .GetAllWithDeleted()
                    .AsNoTracking()
                    .Where(s => s.CreatedOn < twoYearsLimit ||
                        (s.CreatedOn < oneYearLimit &&
                            s.Participant.Scores.All(ps => ps.SubmissionId != s.Id)))
                    .Select(ArchivedSubmission.FromSubmission)
                    .OrderBy(s => s.Id)
                    .Skip(submissionsToSkip)
                    .Take(SubmissionsToTake);

                var submissionIds = new HashSet<int>();

                using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        this.archivesContext.DbContext.Database.Connection.Open();
                        this.archivesContext.DbContext.Configuration.AutoDetectChangesEnabled = false;

                        var count = 0;
                        foreach (var submission in submissionsForArchive)
                        {
                            this.archivesContext.Submissions.Add(submission);

                            if (++count % CommitRateCount == 0)
                            {
                                this.archivesContext.DbContext.SaveChanges();
                            }

                            submissionIds.Add(submission.Id);
                        }

                        this.archivesContext.DbContext.SaveChanges();

                        this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(submissionIds);
                        this.submissionsData.HardDeleteByIds(submissionIds);

                        scope.Complete();
                    }
                    finally
                    {
                        this.archivesContext?.DbContext?.Dispose();
                    }
                }

                submissionsToSkip += SubmissionsToTake;
            } while (submissionsForArchive.Any());
        }
    }
}