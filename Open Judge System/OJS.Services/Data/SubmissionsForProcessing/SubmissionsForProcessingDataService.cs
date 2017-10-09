namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private readonly GenericRepository<SubmissionForProcessing> submissionsForProcessing;

        public SubmissionsForProcessingDataService(GenericRepository<SubmissionForProcessing> submissionsForProcessing)
        {
            this.submissionsForProcessing = submissionsForProcessing;
        }

        public void AddOrUpdateBySubmissionId(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmissionId(submissionId);

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

        public void RemoveBySubmissionId(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmissionId(submissionId);

            if (submissionForProcessing != null)
            {
                this.submissionsForProcessing.Delete(submissionId);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void SetToProcessing(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = true;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void SetToProcessed(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = true;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void ResetProcessingStatus(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public SubmissionForProcessing GetBySubmissionId(int submissionId) =>
            this.submissionsForProcessing.All().FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

        public IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions() =>
            this.submissionsForProcessing.All().Where(sfp => !sfp.Processed && !sfp.Processing);

        public ICollection<int> GetProcessingSubmissionIds() => this.submissionsForProcessing
            .All().Where(sfp => sfp.Processing && !sfp.Processed).Select(sfp => sfp.Id).ToList();

        public void Clean() => this.submissionsForProcessing
            .All()
            .Where(sfp => sfp.Processed && !sfp.Processing)
            .Delete();
    }
}