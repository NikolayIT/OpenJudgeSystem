namespace OJS.Data.Repositories.Contracts
{
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsRepository : IDeletableEntityRepository<Submission>
    {
        bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions);

        bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId);
    }
}