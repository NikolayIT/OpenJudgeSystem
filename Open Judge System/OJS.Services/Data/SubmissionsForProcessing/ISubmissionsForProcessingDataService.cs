namespace OJS.Services.Data.SubmissionsForProcessing
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingDataService : IService
    {
        SubmissionForProcessing GetBySubmissionId(int submissionId);

        void AddOrUpdate(int submissionId);

        void Remove(int submissionId);

        void Clean();
    }
}