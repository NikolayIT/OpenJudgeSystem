namespace OJS.Services.Business.ParticipantScores
{
    using OJS.Services.Common;

    public interface IParticipantScoresBusinessService : IService
    {
        void RecalculateForParticipantByProblem(int participantId, int problemId);

        void NormalizeAllPointsThatExceedAllowedLimit();
    }
}