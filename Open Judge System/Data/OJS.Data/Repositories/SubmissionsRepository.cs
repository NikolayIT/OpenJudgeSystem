namespace OJS.Data.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class SubmissionsRepository : EfDeletableEntityRepository<Submission>, ISubmissionsRepository
    {
        public SubmissionsRepository(DbContext context)
            : base(context)
        {
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