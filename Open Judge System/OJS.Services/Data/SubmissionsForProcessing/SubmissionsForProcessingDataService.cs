namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Data.Entity;
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

        public void AddOrUpdate(IEnumerable<int> submissionIds)
        {
            try
            {
                this.submissionsForProcessing.ContextConfiguration.AutoDetectChangesEnabled = false;
                foreach (var submissionId in submissionIds)
                {
                    var submissionForProcessing = this.GetBySubmissionId(submissionId);

                    if (submissionForProcessing != null)
                    {
                        var entry = this.submissionsForProcessing.GetEntry(submissionForProcessing);
                        entry.Entity.Processed = false;
                        entry.Entity.Processing = false;

                        entry.State = EntityState.Modified;
                    }
                    else
                    {
                        submissionForProcessing = new SubmissionForProcessing
                        {
                            SubmissionId = submissionId
                        };

                        this.submissionsForProcessing.GetEntry(submissionForProcessing).State = EntityState.Added;
                    }
                }
            }
            finally
            {
                this.submissionsForProcessing.SaveChanges();
                this.submissionsForProcessing.ContextConfiguration.AutoDetectChangesEnabled = true;
            }
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
    }
}