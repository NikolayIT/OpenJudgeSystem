namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;

    using Hangfire.Server;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsBusinessService submissionsBusiness;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData,
            ISubmissionsBusinessService submissionsBusiness,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
            this.submissionsBusiness = submissionsBusiness;
            this.backgroundJobs = backgroundJobs;
        }

        public void ArchiveOldSubmissions(PerformContext context)
        {
            this.archivedSubmissionsData.CreateDatabaseIfNotExists();

            var allSubmissionsForArchive = this.submissionsBusiness
                .GetAllForArchiving()
                .Select(ArchivedSubmission.FromSubmission);

            this.archivedSubmissionsData.Add(allSubmissionsForArchive);

             this.backgroundJobs.OnSucceededStateContinueWith<IArchivedSubmissionsBusinessService>(
                 context.BackgroundJob.Id,
                 s => s.HardDeleteArchivedFromMainDatabase());
        }

        public void HardDeleteArchivedFromMainDatabase() =>
            this.archivedSubmissionsData
                .GetAllUndeletedFromMainDatabase()
                .Select(s => s.Id)
                .AsEnumerable()
                .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                .ForEach(submissionIds =>
                    this.HardDeleteSubmissionsByIds(new HashSet<int>(submissionIds)));

        private void HardDeleteSubmissionsByIds(ICollection<int> ids)
        {
            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
            {
                this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(ids);
                this.submissionsData.HardDeleteByIds(ids);

                this.archivedSubmissionsData.SetToHardDeletedFromMainDatabaseByIds(ids);

                scope.Complete();
            }
        }
    }
}