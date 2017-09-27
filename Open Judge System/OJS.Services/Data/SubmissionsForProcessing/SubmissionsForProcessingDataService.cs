namespace OJS.Services.Data.SubmissionsForProcessing
{
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Repositories.Contracts;

    public class SubmissionsForProcessingDataService : ISubmissionsForProcessingDataService
    {
        private readonly ISubmissionsForProcessingRepository submissionsForProcessing;

        public SubmissionsForProcessingDataService(ISubmissionsForProcessingRepository submissionsForProcessing)
        {
            this.submissionsForProcessing = submissionsForProcessing;
        }
        public void Clean()
        {
            this.submissionsForProcessing
                .All()
                .Where(s => s.Processed && !s.Processing)
                .Delete();
        }
    }
}
