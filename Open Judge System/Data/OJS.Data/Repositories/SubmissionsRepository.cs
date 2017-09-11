namespace OJS.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsRepository : DeletableEntityRepository<Submission>, ISubmissionsRepository
    {
        public SubmissionsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public IQueryable<Submission> AllPublic() => 
            this.All()
                .Where(s => s.IsPublic ??
                    (((s.Participant.IsOfficial && s.Problem.Contest.ContestPassword == null) ||
                    (!s.Participant.IsOfficial && s.Problem.Contest.PracticePassword == null)) &&
                    s.Problem.Contest.IsVisible && !s.Problem.Contest.IsDeleted &&
                    s.Problem.ShowResults));

        public IQueryable<Submission> AllForLecturer(string lecturerId)
        {
            var problemsIds = new HashSet<int>(
                this.Context.Contests
                    .Where(c => c.Category.Lecturers.Any(cat => cat.LecturerId == lecturerId) ||
                        c.Lecturers.Any(l => l.LecturerId == lecturerId))
                    .SelectMany(c => c.Problems.Select(p => p.Id)));

            var submissions = this.All()
                .Where(s => s.IsPublic.Value || problemsIds.Contains(s.Problem.Id));

            return submissions;
        }  

        public Submission GetSubmissionForProcessing() =>
                this.All()
                    .Where(s => !s.Processed && !s.Processing)
                    .OrderBy(s => s.Id)
                    .Include(s => s.Problem)
                    .Include(s => s.Problem.Tests)
                    .Include(s => s.Problem.Checker)
                    .Include(s => s.SubmissionType)
                    .FirstOrDefault();

        public bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions)
        {
            var lastSubmission =
                this.All()
                    .Where(s => s.ParticipantId == participantId)
                    .OrderByDescending(s => s.CreatedOn)
                    .Select(s => new { s.Id, s.CreatedOn })
                    .FirstOrDefault();

            if (lastSubmission != null)
            {
                // check if the submission was sent after the submission time limit has passed
                var latestSubmissionTime = lastSubmission.CreatedOn;
                var differenceBetweenSubmissions = DateTime.Now - latestSubmissionTime;
                if (differenceBetweenSubmissions.TotalSeconds < limitBetweenSubmissions)
                {
                    return true;
                }
            }

            return false;
        }

        public IQueryable<Submission> GetLastFiftySubmissions()
        {
            // TODO: add language type
            var submissions = this.AllPublic()
                .OrderByDescending(s => s.CreatedOn)
                .Take(50);

            return submissions;
        }

        public bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId) =>
            this.All().Any(s => s.ProblemId == problemId && s.Participant.UserId == userId && !s.Processed);
    }
}