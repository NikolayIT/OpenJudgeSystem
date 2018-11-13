namespace OJS.Services.Business.ParticipantScores
{
    using OJS.Services.Common;

    public interface IParticipantScoresBusinessService : IService
    {
        void RecalculateForParticipantByProblem(int participantId, int problemId);

        (int updatedSubmissionsCount, int updatedParticipantScoresCount) NormalizeAllPointsThatExceedAllowedLimit();

        void NormalizePointsThatExceedAllowedLimitByContest(int contestId);
    }
}