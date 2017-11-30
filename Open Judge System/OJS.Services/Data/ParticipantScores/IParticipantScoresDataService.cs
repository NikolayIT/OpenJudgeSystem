namespace OJS.Services.Data.ParticipantScores
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IParticipantScoresDataService : IService
    {
        ParticipantScore GetByParticipantIdAndProblemId(int participantId, int problemId);

        ParticipantScore GetByParticipantIdProblemIdAndIsOfficial(int participantId, int problemId, bool isOfficial);

        IQueryable<ParticipantScore> GetAll();

        void SaveBySubmission(Submission submission, bool resetScore = false);

        void DeleteAllByProblem(int problemId);

        void DeleteForParticipantByProblem(int participantId, int problemId);

        void Delete(IEnumerable<ParticipantScore> participantScores);
    }
}