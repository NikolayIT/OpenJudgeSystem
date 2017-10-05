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
            var allProcessingSubmissions = this.submissionsForProcessingData.GetProcessingSubmissions();

            if (allProcessingSubmissions.Any())
            {
                try
                {
                    foreach (var unprocessedSubmission in allProcessingSubmissions)
                    {
                        this.submissionsForProcessingData.ResetForProcessing(unprocessedSubmission.Id);
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