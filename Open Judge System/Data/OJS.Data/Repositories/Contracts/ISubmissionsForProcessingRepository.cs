namespace OJS.Data.Repositories.Contracts
{
    using Data.Contracts;
    using Models;

    public interface ISubmissionsForProcessingRepository : IRepository<SubmissionsForProcessing>
    {
        void AddOrUpdate(int submissionId);

        void Remove(int submissionId);
    }
}
