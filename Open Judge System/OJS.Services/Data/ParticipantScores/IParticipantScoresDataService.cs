namespace OJS.Services.Data.ParticipantScores
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantScoresDataService : IService
    {
        ParticipantScore GetParticipantScore(int participantId, int problemId, bool isOfficial);

        void SaveParticipantScore(Submission submission, bool resetScore = false);

        void DeleteParticipantScores(int problemId);
    }
}