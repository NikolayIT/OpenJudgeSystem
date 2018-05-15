namespace OJS.Services.Data.Submissions
{
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using MissingFeatures;

    using OJS.Common;
    using OJS.Common.Extensions;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsDataService : ISubmissionsDataService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;

        public SubmissionsDataService(IEfDeletableEntityRepository<Submission> submissions) =>
            this.submissions = submissions;

        public Submission GetBestForParticipantByProblem(int participantId, int problemId) =>
            this.GetAllByProblemAndParticipant(problemId, participantId)
                .Where(s => s.Processed)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetAllWithDeleted() => this.submissions.AllWithDeleted();

        public IQueryable<Submission> GetByIdQuery(int id) =>
            this.submissions.All().Where(s => s.Id == id);

        public IQueryable<Submission> GetAllByProblem(int problemId) =>
            this.submissions.All().Where(s => s.ProblemId == problemId);

        public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId) =>
            this.GetAllByProblem(problemId).Where(s => s.ParticipantId == participantId);

        public IEnumerable<int> GetIdsByProblem(int problemId) =>
            this.GetAllByProblem(problemId).Select(s => s.Id);

        public void SetAllToUnprocessedByProblem(int problemId) =>
            this.GetAllByProblem(problemId).Update(s => new Submission{ Processed = false });

        public void DeleteByProblem(int problemId) =>
            this.submissions.Delete(s => s.ProblemId == problemId);

        public void HardDeleteByIds(IEnumerable<int> ids) =>
            ids
                .ChunkBy(GlobalConstants.BatchOperationsChunkSize)
                .ForEach(chunk => this.submissions
                    .HardDelete(s => chunk.Contains(s.Id)));
    }
}