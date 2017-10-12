namespace OJS.Data.Repositories
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsForProcessingRepository : GenericRepository<SubmissionForProcessing>, ISubmissionsForProcessingRepository
    {
        public SubmissionsForProcessingRepository(IOjsDbContext context) 
            : base(context)
        {
        }

        public void AddOrUpdate(int submissionId)
        {
            var submissionForProcessing = this.Context.SubmissionsForProcessing
                .FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

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

                this.Context.SubmissionsForProcessing.Add(submissionForProcessing);
            }
        }

        public void BulkAddOrUpdate(IEnumerable<int> submissionIds)
        {
            try
            {
                this.Context.DbContext.Configuration.AutoDetectChangesEnabled = false;
                foreach (var submissionId in submissionIds)
                {
                    var submissionForProcessing = this.Context.SubmissionsForProcessing
                        .FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

                    if (submissionForProcessing != null)
                    {
                        this.Context.Entry(submissionForProcessing).Entity.Processed = false;
                        this.Context.Entry(submissionForProcessing).Entity.Processing = false;
                        this.Context.Entry(submissionForProcessing).State = EntityState.Modified;
                    }
                    else
                    {
                        submissionForProcessing = new SubmissionForProcessing
                        {
                            SubmissionId = submissionId
                        };

                        this.Context.Entry(submissionForProcessing).State = EntityState.Added;
                    }
                }
            }
            finally
            {
                this.Context.SaveChanges();
                this.Context.DbContext.Configuration.AutoDetectChangesEnabled = true;
            }           
        }

        public void Remove(int submissionId)
        {
            var submissionForProcessing = this.Context.SubmissionsForProcessing
                .FirstOrDefault(sfp => sfp.SubmissionId == submissionId);

            if (submissionForProcessing != null)
            {
                this.Delete(submissionForProcessing.Id);
            }
        }
    }
}