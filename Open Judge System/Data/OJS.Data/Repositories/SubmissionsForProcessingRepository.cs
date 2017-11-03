namespace OJS.Data.Repositories
{
    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsForProcessingRepository :
        EfGenericRepository<SubmissionForProcessing>,
        ISubmissionsForProcessingRepository
    {
        public SubmissionsForProcessingRepository(IOjsDbContext context)
            : base(context)
        {
        }
    }
}