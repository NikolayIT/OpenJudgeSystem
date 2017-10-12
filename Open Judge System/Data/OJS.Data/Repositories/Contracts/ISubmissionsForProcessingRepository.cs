namespace OJS.Data.Repositories.Contracts
{
    using System.Collections.Generic;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsForProcessingRepository : IRepository<SubmissionForProcessing>
    {
        void AddOrUpdate(int submissionId);

        void AddOrUpdate(IEnumerable<int> submissionIds);

        void Remove(int submissionId);
    }
}