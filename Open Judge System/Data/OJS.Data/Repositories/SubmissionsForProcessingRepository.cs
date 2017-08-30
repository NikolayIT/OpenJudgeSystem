namespace OJS.Data.Repositories
{
    using System.Linq;
    using Base;
    using Contracts;
    using Models;

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
                submissionForProcessing = new SubmissionForProcessing()
                {
                    SubmissionId = submissionId
                };
                this.Context.SubmissionsForProcessing.Add(submissionForProcessing);
            }

            this.Context.SaveChanges();      
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
