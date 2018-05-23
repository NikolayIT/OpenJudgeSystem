namespace OJS.Services.Business.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Transactions;

    using OJS.Common;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    public class SubmissionsBusinessService : ISubmissionsBusinessService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;
        private readonly ISubmissionsDataService submissionsData;
        private readonly IParticipantScoresDataService participantScoresData;

        public SubmissionsBusinessService(
            IEfDeletableEntityRepository<Submission> submissions,
            ISubmissionsDataService submissionsData,
            IParticipantScoresDataService participantScoresData)
        {
            this.submissions = submissions;
            this.submissionsData = submissionsData;
            this.participantScoresData = participantScoresData;
        }

        public IQueryable<Submission> GetAllForArchiving()
        {
            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveNonBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

            return this.submissionsData.GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                archiveBestSubmissionsLimit,
                archiveNonBestSubmissionsLimit);
        }

        public void HardDeleteByIds(ICollection<int> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            using (var scope = TransactionsHelper.CreateTransactionScope(IsolationLevel.ReadCommitted))
            {
                this.participantScoresData.RemoveSubmissionIdsBySubmissionIds(ids);
                this.submissions.HardDelete(s => ids.Contains(s.Id));

                scope.Complete();
            }
        }
    }
}