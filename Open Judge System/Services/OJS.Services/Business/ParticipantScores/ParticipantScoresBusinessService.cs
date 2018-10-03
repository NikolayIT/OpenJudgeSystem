namespace OJS.Services.Business.ParticipantScores
{
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.Submissions;

    public class ParticipantScoresBusinessService : IParticipantScoresBusinessService
    {
        private readonly IParticipantScoresDataService participantScoresData;
        private readonly ISubmissionsDataService submissionsData;

        public ParticipantScoresBusinessService(
            IParticipantScoresDataService participantScoresData,
            ISubmissionsDataService submissionsData)
        {
            this.participantScoresData = participantScoresData;
            this.submissionsData = submissionsData;
        }

        public void RecalculateForParticipantByProblem(int participantId, int problemId)
        {
            var submission = this.submissionsData.GetBestForParticipantByProblem(participantId, problemId);

            if (submission != null)
            {
                this.participantScoresData.ResetBySubmission(submission);
            }
            else
            {
                this.participantScoresData.DeleteForParticipantByProblem(participantId, problemId);
            }
        }
    }
}