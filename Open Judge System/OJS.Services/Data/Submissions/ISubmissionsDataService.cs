namespace OJS.Services.Data.Submissions
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsDataService : IService
    {
        Submission GetBestForParticipantByProblem(int participantId, int problemId);

        IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);
    }
}