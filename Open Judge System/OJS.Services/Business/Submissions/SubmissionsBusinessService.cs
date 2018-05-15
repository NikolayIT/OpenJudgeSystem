namespace OJS.Services.Business.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common.Helpers;
    using OJS.Data.Archives;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    using IsolationLevel = System.Transactions.IsolationLevel;

    public class SubmissionsBusinessService : ISubmissionsBusinessService
    {
        private IArchivesDbContext archivesContext;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public SubmissionsBusinessService(
            IArchivesDbContext archivesContext,
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.archivesContext = archivesContext;
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
        }

        public void ArchiveAllExceptBestOlderThanOneYear()
        {
            var submissionsCreatedOnDateLimit = DateTime.Now.AddYears(-3);

            var submissionsForArchive = this.submissionsData
                .GetAllWithDeleted()
                .AsNoTracking()
                .Where(s => s.CreatedOn < submissionsCreatedOnDateLimit &&
                    s.Participant.Scores.All(ps => ps.SubmissionId != s.Id));

            var submissionIds = new List<int>();

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
            {
                try
                {
                    this.archivesContext.DbContext.Database.Connection.Open();
                    this.archivesContext.DbContext.Configuration.AutoDetectChangesEnabled = false;

                    var count = 0;
                    foreach (var submission in submissionsForArchive)
                    {
                        this.archivesContext = AddSubmissionToArchive(
                            this.archivesContext,
                            submission,
                            ++count,
                            100);

                        submissionIds.Add(submission.Id);
                    }

                    this.archivesContext.DbContext.SaveChanges();

                    this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(submissionIds);
                    this.submissionsData.HardDeleteByIds(submissionIds);

                    scope.Complete();
                }
                finally
                {
                    this.archivesContext.DbContext.Configuration.AutoDetectChangesEnabled = true;
                    this.archivesContext.DbContext.Database.Connection.Close();
                    this.archivesContext?.DbContext.Dispose();
                } 
            }
        }

        private static IArchivesDbContext AddSubmissionToArchive(
            IArchivesDbContext context,
            Submission submission,
            int count,
            int commitCount)
        {
            context.Submissions.Add(new ArchivedSubmission(submission));

            if (count % commitCount != 0)
            {
                return context;
            }

            context.DbContext.SaveChanges();

            context.Dispose();
            context = new ArchivesDbContext();
            context.DbContext.Configuration.AutoDetectChangesEnabled = false;

            return context;
        }
    }
}