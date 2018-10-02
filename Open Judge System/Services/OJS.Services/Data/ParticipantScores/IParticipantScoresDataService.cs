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

        IQueryable<ParticipantScore> GetAllByProblem(int problemId);

        void ResetBySubmission(Submission submission);

        void DeleteAllByProblem(int problemId);

        void DeleteForParticipantByProblem(int participantId, int problemId);

        void Delete(IEnumerable<ParticipantScore> participantScores);

        void AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool participantIsOfficial);

        void UpdateBySubmissionAndPoints(ParticipantScore existingScore, int submissionId, int submissionPoints);

        void RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds);
    }
}