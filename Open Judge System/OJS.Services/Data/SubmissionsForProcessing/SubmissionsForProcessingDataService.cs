namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private readonly GenericRepository<SubmissionForProcessing> submissionsForProcessing;

        public SubmissionsForProcessingDataService(GenericRepository<SubmissionForProcessing> submissionsForProcessing)
        {
            this.submissionsForProcessing = submissionsForProcessing;
        }

        public void AddOrUpdateBySubmissionId(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmissionId(submissionId);

            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
            }
            else
            {
                submissionForProcessing = new SubmissionForProcessing
                {
                    SubmissionId = submissionId
                };

                this.submissionsForProcessing.Add(submissionForProcessing);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void RemoveBySubmissionId(int submissionId)
        {
            var submissionForProcessing = this.GetBySubmissionId(submissionId);

            if (submissionForProcessing != null)
            {
                this.submissionsForProcessing.Delete(submissionId);
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void SetToProcessing(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = true;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void SetToProcessed(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = true;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public void ResetForProcessing(int id)
        {
            var submissionForProcessing = this.submissionsForProcessing.GetById(id);
            if (submissionForProcessing != null)
            {
                submissionForProcessing.Processing = false;
                submissionForProcessing.Processed = false;
                this.submissionsForProcessing.SaveChanges();
            }
        }

        public SubmissionForProcessing GetBySubmissionId(int submissionId) =>
            this.submissionsForProcessing.All().FirstOrDefault(s => s.SubmissionId == submissionId);

        public IQueryable<SubmissionForProcessing> GetUnprocessedSubmissions() =>
            this.submissionsForProcessing.All().Where(x => !x.Processed && !x.Processing);

        public IQueryable<SubmissionForProcessing> GetProcessingSubmissions() =>
            this.submissionsForProcessing.All().Where(s => s.Processing && !s.Processed);

        public void Clean() => this.submissionsForProcessing
            .All()
            .Where(s => s.Processed && !s.Processing)
            .Delete();
    }
}