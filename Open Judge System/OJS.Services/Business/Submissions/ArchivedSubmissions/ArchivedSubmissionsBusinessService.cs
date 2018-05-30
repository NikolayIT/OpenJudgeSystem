namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System.Linq;

    using Hangfire.Server;

    using OJS.Data.Models;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsBusinessService submissionsBusiness;
        private readonly IHangfireBackgroundJobService backgroundJobs;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsBusinessService submissionsBusiness,
            IHangfireBackgroundJobService backgroundJobs)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsBusiness = submissionsBusiness;
            this.backgroundJobs = backgroundJobs;
        }

        public void ArchiveOldSubmissions(PerformContext context)
        {
            this.archivedSubmissionsData.CreateDatabaseIfNotExists();

            var allSubmissionsForArchive = this.submissionsBusiness
                .GetAllForArchiving()
                .Select(ArchivedSubmission.FromSubmission);

            this.archivedSubmissionsData.Add(allSubmissionsForArchive);

            this.backgroundJobs.OnSucceededStateContinueWith<ISubmissionsBusinessService>(
                context.BackgroundJob.Id,
                s => s.HardDeleteAllArchived());
        }
    }
}