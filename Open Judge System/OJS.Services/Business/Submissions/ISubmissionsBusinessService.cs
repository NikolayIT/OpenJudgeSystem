namespace OJS.Services.Business.Submissions
{
    using OJS.Services.Common;

    public interface ISubmissionsBusinessService : IService
    {
        void ArchiveAllExceptBestOlderThanOneYear();
    }
}