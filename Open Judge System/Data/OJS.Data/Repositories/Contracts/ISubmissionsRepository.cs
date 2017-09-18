namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsRepository : IDeletableEntityRepository<Submission>
    {
        IQueryable<Submission> AllPublic();

        IQueryable<Submission> AllPublicWithLecturerContests(string lecturerId);

        Submission GetSubmissionForProcessing();

        bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions);

        IQueryable<Submission> GetLastFiftySubmissions();

        bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId);
    }
}
