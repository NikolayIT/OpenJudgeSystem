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

        public SubmissionForProcessing GetBySubmissionId(int submissionId) =>
            this.submissionsForProcessing.All().FirstOrDefault(s => s.SubmissionId == submissionId);

        public void AddOrUpdate(int submissionId)
        {
            this.AddOrUpdateWithNoSaveChanges(submissionId);
            this.submissionsForProcessing.SaveChanges();
        }

        // Used optimization from this article https://msdn.microsoft.com/en-us/data/jj556205 for better performance
        public void AddOrUpdate(IEnumerable<int> submissionIds)
        {
            try
            {
                this.submissionsForProcessing.ContextConfiguration.AutoDetectChangesEnabled = false;
                foreach (var submissionId in submissionIds)
                {
                    this.AddOrUpdateWithNoSaveChanges(submissionId);
                }
            }
            finally
            {
                this.submissionsForProcessing.ContextConfiguration.AutoDetectChangesEnabled = true;
            }

            this.submissionsForProcessing.SaveChanges();
        }

        public void Remove(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmissionId(submissionId);

            if (submissionForProcessing != null)
            {
                this.submissionsForProcessing.Delete(submissionId);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void Clean()
        {
            this.submissionsForProcessing
                .All()
                .Where(s => s.Processed && !s.Processing)
                .Delete();
        }

        private void AddOrUpdateWithNoSaveChanges(int submissionId)
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
            }
        }
    }
}