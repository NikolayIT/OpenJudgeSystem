namespace OJS.Services.Data.Submissions
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsDataService : IService
    {
        Submission GetLastBestSubmissionForParticipantByProblem(int participantId, int problemId);
    }
}