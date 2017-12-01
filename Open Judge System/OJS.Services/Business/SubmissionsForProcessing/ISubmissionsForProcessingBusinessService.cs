namespace OJS.Services.Business.SubmissionsForProcessing
{
    using log4net;

    using OJS.Services.Common;

    public interface ISubmissionsForProcessingBusinessService : IService
    {
        void ResetAllProcessingSubmissions(ILog logger);
    }
}