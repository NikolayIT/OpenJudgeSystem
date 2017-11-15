namespace OJS.Services.Data.Submissions
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsDataService : IService
    {
        Submission GetBestSubmission(int participantId, int problemId);
    }
}