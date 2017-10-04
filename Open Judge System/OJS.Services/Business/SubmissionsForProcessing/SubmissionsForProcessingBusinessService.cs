namespace OJS.Services.Business.SubmissionsForProcessing
{
    using System;
    using System.Linq;

    using log4net;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;

    public class SubmissionsForProcessingBusinessService : ISubmissionsForProcessingBusinessService
    {
        private readonly GenericRepository<SubmissionForProcessing> submissionsForProcessing;

        public SubmissionsForProcessingBusinessService(
            GenericRepository<SubmissionForProcessing> submissionsForProcessing)
        {
            this.submissionsForProcessing = submissionsForProcessing;
        }

        public IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions() =>
            this.submissionsForProcessing
                .All()
                .Where(x => !x.Processed && !x.Processing);

        public void SetToProcessing(SubmissionForProcessing submissionForProcessing)
        {
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = true;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void SetToProcessed(SubmissionForProcessing submissionForProcessing)
        {
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processed = true;
                submissionForProcessing.Processing = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        /// <summary>
        /// Sets the Processing property to False for all submissions
        /// thus ensuring that the worker will process them eventually instead
        /// of getting stuck in perpetual "Processing..." state
        /// </summary>
        public void ResetAllProcessingSubmissions(ILog logger)
        {
            var allProcessingSubmissions = this.submissionsForProcessing
                .All()
                .Where(s => s.Processing && !s.Processed);

            if (allProcessingSubmissions.Any())
            {
                try
                {
                    foreach (var unprocessedSubmission in allProcessingSubmissions)
                    {
                        unprocessedSubmission.Processing = false;
                    }

                    this.submissionsForProcessing.SaveChanges();
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Clearing Processing submissions failed with exception {0}", ex.Message);
                }
            }
        }
    }
}