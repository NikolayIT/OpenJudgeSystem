namespace OJS.Services.Business.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class SubmissionsBusinessService : ISubmissionsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public SubmissionsBusinessService(
            IEfDeletableEntityRepository<Submission> submissions,
            ISubmissionsDataService submissionsData,
            IArchivedSubmissionsDataService archivedSubmissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.submissions = submissions;
            this.submissionsData = submissionsData;
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.participantScoresData = participantScoresData;
        }

        public IQueryable<Submission> GetAllForArchiving()
        {
            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveNonBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

            return this.submissionsData
                .GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                    archiveBestSubmissionsLimit,
                    archiveNonBestSubmissionsLimit);
        }

        public void HardDeleteAllArchived() =>
            this.archivedSubmissionsData
                .GetAllUndeletedFromMainDatabase()
                .Select(s => s.Id)
                .AsEnumerable()
                .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                .ForEach(submissionIds =>
                    this.HardDeleteByArchivedIds(new HashSet<int>(submissionIds)));

        private void HardDeleteByArchivedIds(ICollection<int> ids)
        {
            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
            {
                this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(ids);
                this.submissions.HardDelete(s => ids.Contains(s.Id));

                this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(ids);

                scope.Complete();
            }
        }
    }
}