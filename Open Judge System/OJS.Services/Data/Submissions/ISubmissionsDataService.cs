namespace OJS.Services.Data.Submissions
{
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface ISubmissionsDataService : IService
    {
        Submission GetBestForParticipantByProblem(int participantId, int problemId);

        IQueryable<Submission> GetByIdQuery(int id);

        IQueryable<Submission> GetAllByProblem(int problemId);

        IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId);

        IEnumerable<int> GetIdsByProblem(int problemId);

        void SetAllToUnprocessedByProblem(int problemId);

        void DeleteByProblem(int problemId);

        void ArchiveById(int id);
    }
}