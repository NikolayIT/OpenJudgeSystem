namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsForProcessingRepository : IRepository<SubmissionForProcessing>
    {
        void AddOrUpdate(int submissionId);

        void Remove(int submissionId);
    }
}
