namespace OJS.Data.Repositories
{
    using System.Linq;
    using Base;
    using Contracts;
    using Models;

    public class SubmissionsForProcessingRepository : GenericRepository<SubmissionsForProcessing>, ISubmissionsForProcessingRepository
    {
        public SubmissionsForProcessingRepository(IOjsDbContext context) 
            : base(context)
        {
        }

        public void AddOrUpdateSubmissionForProcessing(int submissionId)
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
                submissionForProcessing = new SubmissionsForProcessing()
                {
                    SubmissionId = submissionId
                };
                this.Context.SubmissionsForProcessing.Add(submissionForProcessing);
            }

            this.Context.SaveChanges();      
        }
    }
}
