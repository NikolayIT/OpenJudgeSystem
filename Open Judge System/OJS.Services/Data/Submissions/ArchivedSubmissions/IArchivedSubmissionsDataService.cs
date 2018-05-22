namespace OJS.Services.Data.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IArchivedSubmissionsDataService : IService
    {
        IQueryable<ArchivedSubmission> GetAllBySubmissionIds(IEnumerable<int> submissionIds);

        void Add(IEnumerable<ArchivedSubmission> entities);
    }
}