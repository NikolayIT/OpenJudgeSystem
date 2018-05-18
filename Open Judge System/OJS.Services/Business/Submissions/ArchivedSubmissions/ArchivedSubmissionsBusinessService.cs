namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public ArchivedSubmissionsBusinessService(
            IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions,
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.archivedSubmissions = archivedSubmissions;
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
        }

        public void ArchiveOldSubmissions()
        {
            this.archivedSubmissions.CreateDatabaseIfNotExists();

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

                var notArchivedsubmissionIds = submissionIds.Except(
                    this.archivedSubmissions
                        .All()
                        .AsNoTracking()
                        .Where(s => submissionIds.Contains(s.Id))
                        .Select(s => s.Id));

                this.archivedSubmissions.Add(
                    submissionsForArchive.Where(s => notArchivedsubmissionIds.Contains(s.Id)));

                using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
                {
                    this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(submissionIds);
                    this.submissionsData.HardDeleteByIds(submissionIds);

                    scope.Complete();
                }

                submissionsToSkip += SubmissionsToTake;
            } while (submissionsForArchive.Any());
        }
    }
}