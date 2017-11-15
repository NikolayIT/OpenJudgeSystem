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

        public void RecalculateParticipantScore(int participantId, int problemId)
        {
            var submission = this.submissionsData.GetLastBestSubmissionForParticipantByProblem(
                participantId,
                problemId);

            if (submission != null)
            {
                this.participantScoresData.SaveParticipantScore(submission, true);
            }
            else
            {
                this.participantScoresData.DeleteParticipantScore(participantId, problemId);
            }
        }
    }
}