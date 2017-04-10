namespace OJS.Data.Repositories.Contracts
{
    using Data.Contracts;
    using OJS.Data.Models;

    public interface IParticipantScoresRepository : IRepository<ParticipantScore>
    {
        void SaveParticipantScore(Submission submission, bool resetScore = false);

        void RecalculateParticipantScore(int participantId, int problemId);

        void DeleteParticipantScore(Submission submission);

        void DeleteParticipantScores(int problemId);
    }
}
