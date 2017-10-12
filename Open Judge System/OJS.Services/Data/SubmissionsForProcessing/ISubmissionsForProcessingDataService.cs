namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Collections.Generic;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingDataService : IService
    {
        SubmissionForProcessing GetBySubmissionId(int submissionId);

        void AddOrUpdate(int submissionId);

        void AddOrUpdate(IEnumerable<int> submissionIds);

        void Remove(int submissionId);

        void Clean();
    }
}