namespace OJS.Services.Data.Submissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using EntityFramework.Extensions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsDataService : ISubmissionsDataService
    {
        private readonly IEfDeletableEntityRepository<Submission> submissions;

        public SubmissionsDataService(IEfDeletableEntityRepository<Submission> submissions) =>
            this.submissions = submissions;

        public Submission GetById(int id) => this.submissions.GetById(id);

        public Submission GetBestForParticipantByProblem(int participantId, int problemId) =>
            this.GetAllByProblemAndParticipant(problemId, participantId)
                .Where(s => s.Processed)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.Id)
                .FirstOrDefault();

        public IQueryable<Submission> GetAll() =>
            this.submissions.All();

        public IQueryable<Submission> GetByIdQuery(int id) =>
            this.GetAll()
                .Where(s => s.Id == id);

        public IQueryable<Submission> GetAllByProblem(int problemId) =>
            this.GetAll()
                .Where(s => s.ProblemId == problemId);

        public IQueryable<Submission> GetAllByProblemAndParticipant(int problemId, int participantId) =>
            this.GetAllByProblem(problemId)
                .Where(s => s.ParticipantId == participantId);

        public IQueryable<Submission> GetAllFromContestsByLecturer(string lecturerId) =>
            this.GetAll()
                .Where(s =>
                    (s.IsPublic.HasValue && s.IsPublic.Value) ||
                    s.Problem.ProblemGroup.Contest.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    s.Problem.ProblemGroup.Contest.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public IQueryable<Submission> GetAllCreatedBeforeDateAndNonBestCreatedBeforeDate(
            DateTime createdBeforeDate,
            DateTime nonBestCreatedBeforeDate) =>
            this.submissions
                .AllWithDeleted()
                .Where(s => s.CreatedOn < createdBeforeDate ||
                    (s.CreatedOn < nonBestCreatedBeforeDate &&
                        s.Participant.Scores.All(ps => ps.SubmissionId != s.Id)));

        public IQueryable<Submission> GetAllHavingPointsExceedingLimit() =>
            this.GetAll()
                .Where(s => s.Points > s.Problem.MaximumPoints);

        public IEnumerable<int> GetIdsByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Select(s => s.Id);

        public bool IsOfficialById(int id) =>
            this.GetByIdQuery(id)
                .Any(s => s.Participant.IsOfficial);

        public void SetAllToUnprocessedByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Update(s => new Submission
                {
                    Processed = false
                });

        public void DeleteByProblem(int problemId) =>
            this.submissions.Delete(s => s.ProblemId == problemId);

        public void Update(Submission submission)
        {
            this.submissions.Update(submission);
            this.submissions.SaveChanges();
        }

        public void RemoveTestRunsCacheByProblem(int problemId) =>
            this.GetAllByProblem(problemId)
                .Update(s => new Submission
                {
                    TestRunsCache = null
                });
    }
}