namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Transactions;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
        }

        public void ArchiveOldSubmissions()
        {
            const int SubmissionsToTake = GlobalConstants.BatchOperationsChunkSize;
            var submissionsToSkip = 0;

            IQueryable<ArchivedSubmission> submissionsForArchive;

            do
            {
                submissionsForArchive = this.submissionsData
                    .GetAllForArchiving()
                    .AsNoTracking()
                    .Select(ArchivedSubmission.FromSubmission)
                    .OrderBy(s => s.Id)
                    .Skip(submissionsToSkip)
                    .Take(SubmissionsToTake);

                var submissionIds = new HashSet<int>(submissionsForArchive.Select(s => s.Id));

                var archivedSubmissionIds = this.archivedSubmissionsData
                    .GetAllBySubmissionIds(submissionIds)
                    .AsNoTracking()
                    .Select(s => s.Id);

                var notArchivedsubmissionIds = new HashSet<int>(submissionIds.Except(archivedSubmissionIds));

                this.archivedSubmissionsData.Add(
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