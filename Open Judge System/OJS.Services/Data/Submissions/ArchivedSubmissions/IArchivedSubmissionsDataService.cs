namespace OJS.Services.Data.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IArchivedSubmissionsDataService : IService
    {
        IQueryable<ArchivedSubmission> GetAllUndeletedFromMainDatabase();

        void Add(IEnumerable<ArchivedSubmission> entities);

        void SetToHardDeletedFromMainDatabaseByIds(IEnumerable<int> ids);

        void CreateDatabaseIfNotExists();
    }
}