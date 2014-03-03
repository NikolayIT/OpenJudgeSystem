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

        public IQueryable<Submission> AllPublic()
        {
            return this.All()
                .Where(x =>
                    ((x.Participant.IsOfficial && x.Problem.Contest.ContestPassword == null) ||
                     (!x.Participant.IsOfficial && x.Problem.Contest.PracticePassword == null))
                    && x.Problem.Contest.IsVisible && !x.Problem.Contest.IsDeleted
                    && x.Problem.ShowResults);
        }
        
        public Submission GetSubmissionForProcessing()
        {
            var submission =
                       this.All()
                           .Where(x => !x.Processed && !x.Processing)
                           .OrderBy(x => x.Id)
                           .Include(x => x.Problem)
                           .Include(x => x.Problem.Tests)
                           .Include(x => x.Problem.Checker)
                           .Include(x => x.SubmissionType)
                           .FirstOrDefault();

            return submission;
        }

        public bool HasSubmissionTimeLimitPassedForParticipant(int participantId, int limitBetweenSubmissions)
        {
            var lastSubmission =
                this.All()
                    .Where(x => x.ParticipantId == participantId)
                    .OrderBy(x => x.CreatedOn)
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
    }
}
