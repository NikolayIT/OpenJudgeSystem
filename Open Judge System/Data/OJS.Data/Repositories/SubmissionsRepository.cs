using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace OJS.Data.Repositories
{
    using System;
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
                .Where(x => x.IsPublic 
                    ?? (((x.Participant.IsOfficial && x.Problem.Contest.ContestPassword == null) ||
                    (!x.Participant.IsOfficial && x.Problem.Contest.PracticePassword == null)) &&
                     x.Problem.Contest.IsVisible && !x.Problem.Contest.IsDeleted &&
                     x.Problem.ShowResults));

        public IQueryable<Submission> AllForLecturer(string lecturerId)
        {
            var problemsIds = new HashSet<int>(
                this.Context
                        .Contests
                        .Where(c => c.Category.Lecturers.Any(cat => cat.LecturerId == lecturerId) ||
                                    c.Lecturers.Any(l => l.LecturerId == lecturerId))
                        .SelectMany(c => c.Problems.Select(p => p.Id)));

            var submissions = this.All()
                .Where(s => s.IsPublic.Value || problemsIds.Contains(s.Problem.Id));

            return submissions;
        }  

        public Submission GetSubmissionForProcessing() =>
                this.All()
                    .Where(x => !x.Processed && !x.Processing)
                    .OrderBy(x => x.Id)
                    .Include(x => x.Problem)
                    .Include(x => x.Problem.Tests)
                    .Include(x => x.Problem.Checker)
                    .Include(x => x.SubmissionType)
                    .FirstOrDefault();

        public bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions)
        {
            var lastSubmission =
                this.All()
                    .Where(x => x.ParticipantId == participantId)
                    .OrderByDescending(x => x.CreatedOn)
                    .Select(x => new { x.Id, x.CreatedOn })
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
                .OrderByDescending(x => x.CreatedOn)
                .Take(50);

            return submissions;
        }

        public bool HasUserNotProcessedSubmissionForProblem(int problemId, string userId) =>
            this.All().Any(s => s.ProblemId == problemId && s.Participant.UserId == userId && !s.Processed);
    }
}
