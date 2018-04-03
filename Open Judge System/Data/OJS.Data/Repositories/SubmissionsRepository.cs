namespace OJS.Data.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsRepository : EfDeletableEntityRepository<Submission>, ISubmissionsRepository
    {
        public SubmissionsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public IQueryable<Submission> AllPublic() => 
            this.All()
                .Where(s => s.IsPublic ??
                    (((s.Participant.IsOfficial && s.Problem.ProblemGroup.Contest.ContestPassword == null) ||
                        (!s.Participant.IsOfficial && s.Problem.ProblemGroup.Contest.PracticePassword == null)) &&
                    s.Problem.ProblemGroup.Contest.IsVisible &&
                    !s.Problem.ProblemGroup.Contest.IsDeleted &&
                    s.Problem.ShowResults));

        public IQueryable<Submission> AllPublicWithLecturerContests(string lecturerId)
        {
            var problemsIds = new HashSet<int>(
                this.Context.Contests
                    .Where(c => c.Category.Lecturers.Any(cat => cat.LecturerId == lecturerId) ||
                        c.Lecturers.Any(l => l.LecturerId == lecturerId))
                    .SelectMany(c => c.ProblemGroups.SelectMany(pg => pg.Problems).Select(p => p.Id)));

            var submissions =
                this.All()
                    .Where(s => s.IsPublic.Value || problemsIds.Contains(s.Problem.Id));

            return submissions;
        }

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

        public bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId) =>
            this.All().Any(s => s.ProblemId == problemId && s.Participant.UserId == userId && !s.Processed);
    }
}