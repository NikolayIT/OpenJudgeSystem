namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private GenericRepository<SubmissionForProcessing> SubmissionsForProcessing { get; }

        public SubmissionsForProcessingDataService(GenericRepository<SubmissionForProcessing> submissionsForProcessing)
        {
            this.SubmissionsForProcessing = submissionsForProcessing;
        }

        public SubmissionForProcessing GetById(int id) => this.SubmissionsForProcessing.GetById(id);

        public void AddOrUpdate(int submissionId)
        {
            var submissionForProcessing = this.SubmissionsForProcessing.GetById(submissionId);

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

                this.SubmissionsForProcessing.Add(submissionForProcessing);
                this.SubmissionsForProcessing.SaveChanges();
            }
        }

        public void Remove(int submissionId)
        {
            var submissionForProcessing = this.SubmissionsForProcessing.GetById(submissionId);

            if (submissionForProcessing != null)
            {
                this.SubmissionsForProcessing.Delete(submissionId);
                this.SubmissionsForProcessing.SaveChanges();
            }
        }

        public void Clean()
        {
            this.SubmissionsForProcessing
                .All()
                .Where(s => s.Processed && !s.Processing)
                .Delete();
        }
    }
}