namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingDataService : IService
    {
        SubmissionForProcessing GetBySubmission(int submissionId);

        IQueryable<SubmissionForProcessing> GetAllUnprocessed();

        ICollection<int> GetIdsOfAllProcessing();

        void AddOrUpdateBySubmissionIds(ICollection<int> submissionIds);

        void AddOrUpdateBySubmission(int submissionId);

        void RemoveBySubmission(int submissionId);

        void ResetProcessingStatusById(int id);

        void Clean();

        void Update(SubmissionForProcessing submissionForProcessing);
    }
}