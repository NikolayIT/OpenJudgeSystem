namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingDataService : IService
    {
        SubmissionForProcessing GetBySubmissionId(int submissionId);

        IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions();

        IQueryable<SubmissionForProcessing> GetProcessingSubmissions();

        void AddOrUpdateBySubmissionId(int submissionId);

        void RemoveBySubmissionId(int submissionId);

        void SetToProcessing(int id);

        void SetToProcessed(int id);

        void ResetForProcessing(int id);

        void Clean();
    }
}