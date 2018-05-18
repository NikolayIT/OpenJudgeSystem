namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using OJS.Services.Common;

    public interface IArchivedSubmissionsBusinessService : IService
    {
        void ArchiveOldSubmissions();
    }
}