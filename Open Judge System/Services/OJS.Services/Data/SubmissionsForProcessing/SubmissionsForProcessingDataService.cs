namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Linq;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Common.Helpers;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private readonly IEfGenericRepository<SubmissionForProcessing> submissionsForProcessing;

        public SubmissionsForProcessingDataService(IEfGenericRepository<SubmissionForProcessing> submissionsForProcessing) =>
            this.submissionsForProcessing = submissionsForProcessing;

        public SubmissionForProcessing GetBySubmission(int submissionId) =>
            this.submissionsForProcessing
                .All()
                .FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

        public IQueryable<SubmissionForProcessing> GetAllUnprocessed() =>
            this.submissionsForProcessing
                .All()
                .Where(sfp => !sfp.Processed && !sfp.Processing);

        public ICollection<int> GetIdsOfAllProcessing() =>
            this.submissionsForProcessing
                .All()
                .Where(sfp => sfp.Processing && !sfp.Processed)
                .Select(sfp => sfp.Id)
                .ToList();

        public void AddOrUpdateBySubmissionIds(ICollection<int> submissionIds)
        {
            var newSubmissionsForProcessing = submissionIds
                .Select(sId => new SubmissionForProcessing
                {
                    SubmissionId = sId
                });

            using (var scope = TransactionsHelper.CreateTransactionScope())
            {
                submissionIds
                    .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                    .ForEach(chunk => this.submissionsForProcessing
                        .Delete(sfp => chunk.Contains(sfp.SubmissionId)));

                this.submissionsForProcessing.Add(newSubmissionsForProcessing);

                scope.Complete();
            }
        }

        public void AddOrUpdateBySubmission(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmission(submissionId);

            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
            }
            else
            {
                submissionForProcessing = new SubmissionForProcessing
                {
                    SubmissionId = submissionId
                };

                this.submissionsForProcessing.Add(submissionForProcessing);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void RemoveBySubmission(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmission(submissionId);

            if (submissionForProcessing != null)
            {
                this.submissionsForProcessing.Delete(submissionId);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void ResetProcessingStatusById(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void Clean() =>
            this.submissionsForProcessing.Delete(sfp => sfp.Processed && !sfp.Processing);

        public void Update(SubmissionForProcessing submissionForProcessing)
        {
            this.submissionsForProcessing.Update(submissionForProcessing);
            this.submissionsForProcessing.SaveChanges();
        }
    }
}