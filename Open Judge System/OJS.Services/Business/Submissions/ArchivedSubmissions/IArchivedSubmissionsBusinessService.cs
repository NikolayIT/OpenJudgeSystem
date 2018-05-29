namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using Hangfire.Server;

    using OJS.Services.Common;

    public interface IArchivedSubmissionsBusinessService : IArchivesService
    {
        void ArchiveOldSubmissions(PerformContext context);

        void HardDeleteArchivedFromMainDatabase();
    }
}