namespace OJS.Services.Business.SubmissionsForProcessing
{
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingBusinessService : IService
    {
        void ResetAllProcessingSubmissions();
    }
}