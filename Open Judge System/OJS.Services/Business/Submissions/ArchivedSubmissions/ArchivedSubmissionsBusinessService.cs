namespace OJS.Services.Business.Submissions.ArchivedSubmissions
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Data.Submissions.ArchivedSubmissions;

    public class ArchivedSubmissionsBusinessService : IArchivedSubmissionsBusinessService
    {
        private readonly IArchivedSubmissionsDataService archivedSubmissionsData;
        private readonly ISubmissionsBusinessService submissionsBusiness;

        public ArchivedSubmissionsBusinessService(
            IArchivedSubmissionsDataService archivedSubmissionsData,
            ISubmissionsBusinessService submissionsBusiness)
        {
            this.archivedSubmissionsData = archivedSubmissionsData;
            this.submissionsBusiness = submissionsBusiness;
        }

        public void ArchiveOldSubmissions()
        {
            const int SubmissionsToTake = 200;
            var submissionsToSkip = 0;

            var allSubmissionsForArchive = this.submissionsBusiness
                .GetAllForArchiving()
                .AsNoTracking()
                .Select(ArchivedSubmission.FromSubmission)
                .OrderBy(s => s.Id);

            do
            {
                var submissionsForArchive = allSubmissionsForArchive
                    .Skip(submissionsToSkip)
                    .Take(SubmissionsToTake)
                    .ToList();

                var submissionIds = new HashSet<int>(submissionsForArchive.Select(s => s.Id));

                var archivedSubmissionIds = this.archivedSubmissionsData
                    .GetAllBySubmissionIds(submissionIds)
                    .AsNoTracking()
                    .Select(s => s.Id);

                var notArchivedsubmissionIds = new HashSet<int>(submissionIds.Except(archivedSubmissionIds));

                this.archivedSubmissionsData.Add(
                    submissionsForArchive.Where(s => notArchivedsubmissionIds.Contains(s.Id)));

                this.submissionsBusiness.HardDeleteByIds(submissionIds);

                submissionsToSkip += SubmissionsToTake;
            } while (allSubmissionsForArchive.Any());
        }
    }
}