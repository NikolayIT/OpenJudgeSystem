namespace OJS.Data.Repositories.Contracts
{
    using Data.Contracts;
    using Models;

    public interface ISubmissionsForProcessingRepository : IRepository<SubmissionForProcessing>
    {
        void AddOrUpdate(int submissionId);

        void Remove(int submissionId);
    }
}
