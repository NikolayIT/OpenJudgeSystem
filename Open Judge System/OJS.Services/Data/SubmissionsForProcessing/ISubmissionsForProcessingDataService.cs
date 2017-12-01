namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingDataService : IService
    {
        SubmissionForProcessing GetBySubmissionId(int submissionId);

        IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions();

        ICollection<int> GetProcessingSubmissionIds();

        void AddOrUpdate(IEnumerable<int> submissionIds);

        void AddOrUpdateBySubmissionId(int submissionId);

        void RemoveBySubmissionId(int submissionId);

        void SetToProcessing(int id);

        void SetToProcessed(int id);

        void ResetProcessingStatus(int id);

        void Clean();
    }
}