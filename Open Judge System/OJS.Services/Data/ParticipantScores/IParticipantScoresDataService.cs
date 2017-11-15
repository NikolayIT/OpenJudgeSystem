namespace OJS.Services.Data.ParticipantScores
{
    using System.Linq;
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantScoresDataService : IService
    {
        ParticipantScore Get(int participantId, int problemId);

        ParticipantScore Get(int participantId, int problemId, bool isOfficial);

        IQueryable<ParticipantScore> GetAll();

        bool Save(Submission submission, bool resetScore = false);

        void DeleteAllByProblem(int problemId);

        void Delete(int participantId, int problemId);

        void Delete(ParticipantScore participantScore);
    }
}