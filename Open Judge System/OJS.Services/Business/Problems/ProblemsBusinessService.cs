namespace OJS.Services.Business.Problems
{
    using System.Linq;
    using System.Transactions;

    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;
    using OJS.Services.Data.SubmissionsForProcessing;

    public class ProblemsBusinessService : IProblemsBusinessService
    {
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsDataService submissionsData;
        private readonly ISubmissionsForProcessingDataService submissionsForProcessingData;

        public ProblemsBusinessService(
            IParticipantScoresDataService participantScoresData,
            ISubmissionsDataService submissionsData,
            ISubmissionsForProcessingDataService submissionsForProcessingData)
        {
            this.participantScoresData = participantScoresData;
            this.submissionsData = submissionsData;
            this.submissionsForProcessingData = submissionsForProcessingData;
        }

        public void RetestById(int id)
        {
            var submissionIds = this.submissionsData.GetIdsByProblem(id).ToList();

            using (var scope = new TransactionScope())
            {
                this.participantScoresData.DeleteAllByProblem(id);

                this.submissionsData.SetAllToUnprocessedByProblem(id);

                this.submissionsForProcessingData.AddOrUpdate(submissionIds);

                scope.Complete();
            }
        }
    }
}