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

        IQueryable<ParticipantScore> GetAllHavingPointsExceedingLimit();

        void ResetBySubmission(Submission submission);

        void DeleteAllByProblem(int problemId);

        void DeleteForParticipantByProblem(int participantId, int problemId);

        void Delete(IEnumerable<ParticipantScore> participantScores);

        void AddBySubmissionByUsernameAndIsOfficial(Submission submission, string username, bool isOfficial);

        void UpdateBySubmissionAndPoints(
            ParticipantScore participantScore,
            int? submissionId,
            int submissionPoints);

        void RemoveSubmissionIdsBySubmissionIds(IEnumerable<int> submissionIds);
    }
}