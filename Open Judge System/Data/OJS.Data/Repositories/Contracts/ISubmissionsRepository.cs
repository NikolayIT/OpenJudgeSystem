namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsRepository : IDeletableEntityRepository<Submission>
    {
        IQueryable<Submission> AllPublic();

        Submission GetSubmissionForProcessing();

        bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions);

        IQueryable<Submission> GetLastFiftySubmissions();
    }
}
