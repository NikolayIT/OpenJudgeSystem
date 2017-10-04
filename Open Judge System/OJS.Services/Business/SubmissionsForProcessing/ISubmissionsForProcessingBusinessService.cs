namespace OJS.Services.Business.SubmissionsForProcessing
{
    using System.Linq;

    using log4net;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsForProcessingBusinessService : IService
    {
        void ResetAllProcessingSubmissions(ILog logger);

        IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions();

        void SetToProcessing(SubmissionForProcessing submissionForProcessing);

        void SetToProcessed(SubmissionForProcessing submissionForProcessing);
    }
}