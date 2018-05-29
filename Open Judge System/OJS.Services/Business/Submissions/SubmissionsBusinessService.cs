namespace OJS.Services.Business.Submissions
{
    using System;
    using System.Linq;

    using OJS.Common;
    using OJS.Data.Models;
    using OJS.Services.Data.Submissions;

    public class SubmissionsBusinessService : ISubmissionsBusinessService
    {
        private readonly ISubmissionsDataService submissionsData;

        public SubmissionsBusinessService(
            ISubmissionsDataService submissionsData) =>
                this.submissionsData = submissionsData;

        public IQueryable<Submission> GetAllForArchiving()
        {
            var archiveBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.BestSubmissionEligibleForArchiveAgeInYears);

            var archiveNonBestSubmissionsLimit = DateTime.Now.AddYears(
                -GlobalConstants.NonBestSubmissionEligibleForArchiveAgeInYears);

            return this.submissionsData
                .GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
                    archiveBestSubmissionsLimit,
                    archiveNonBestSubmissionsLimit);
        }
    }
}