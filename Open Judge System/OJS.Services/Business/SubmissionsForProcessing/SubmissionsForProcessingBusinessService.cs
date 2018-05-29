namespace OJS.Services.Business.SubmissionsForProcessing
{
    using System;
    using System.Linq;

    using log4net;

    using OJS.Services.Data.SubmissionsForProcessing;

    public class SubmissionsForProcessingBusinessService : ISubmissionsForProcessingBusinessService
    {
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;

        public SubmissionsForProcessingBusinessService(
            ISubmissionsForProcessingDataService submissionsForProcessingData)
        {
            this.submissionsForProcessingData = submissionsForProcessingData;
        }

        /// <summary>
        /// Sets the Processing property to False for all submissions
        /// thus ensuring that the worker will process them eventually instead
        /// of getting stuck in perpetual "Processing..." state
        /// </summary>
        public void ResetAllProcessingSubmissions(ILog logger)
        {
            var allProcessingSubmissionIds = this.submissionsForProcessingData.GetIdsOfAllProcessing();

            if (allProcessingSubmissionIds.Count > 0)
            {
                try
                {
                    foreach (var submissionForProcessingId in allProcessingSubmissionIds)
                    {
                        this.submissionsForProcessingData.ResetProcessingStatusById(submissionForProcessingId);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat($"Clearing Processing submissions failed with exception {ex.Message}");
                }
            }
        }
    }
}