namespace OJS.Data.Repositories.Contracts
{
    using Data.Contracts;
    using Models;

    public interface ISubmissionsForProcessingRepository : IRepository<SubmissionsForProcessing>
    {
        void AddOrUpdateSubmissionForProcessing(int submissionId);
    }
}
