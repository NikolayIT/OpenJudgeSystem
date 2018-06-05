namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using Hangfire.Server;

    using OJS.Services.Common;

    public interface IArchivedSubmissionsBusinessService : IService
    {
        void ArchiveOldSubmissions(PerformContext context);
    }
}