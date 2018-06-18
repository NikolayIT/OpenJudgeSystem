namespace OJS.Services.Data.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Archives.Repositories.Contracts;
    using OJS.Data.Models;

    public class ArchivedSubmissionsDataService : IArchivedSubmissionsDataService
    {
        private readonly IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions;

        public ArchivedSubmissionsDataService(
            IArchivesGenericRepository<ArchivedSubmission> archivedSubmissions) =>
                this.archivedSubmissions = archivedSubmissions;

        public IQueryable<ArchivedSubmission> GetAllUndeletedFromMainDatabase() =>
            this.archivedSubmissions
                .All()
                .Where(s => !s.IsHardDeletedFromMainDatabase);

        public void Add(IEnumerable<ArchivedSubmission> entities) =>
            this.archivedSubmissions.Add(entities);

        public void SetToHardDeletedFromMainDatabaseByIds(IEnumerable<int> ids) =>
            this.archivedSubmissions
                .Update(
                    s => ids.Contains(s.Id),
                    s => new ArchivedSubmission
                    {
                        IsHardDeletedFromMainDatabase = true
                    });

        public void CreateDatabaseIfNotExists() =>
            this.archivedSubmissions.CreateDatabaseIfNotExists();
    }
}