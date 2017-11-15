namespace OJS.Services.Business.ParticipantScores
{
    using OJS.Services.Common;

    public interface IParticipantScoresBusinessService : IService
    {
        void RecalculateParticipantScore(int participantId, int problemId);
    }
}