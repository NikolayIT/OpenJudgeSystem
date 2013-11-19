namespace OJS.Data.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
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
                .Where(x => !(x.Problem.Contest.StartTime <= DateTime.Now && DateTime.Now <= x.Problem.Contest.EndTime)
                    && ((x.Participant.IsOfficial && x.Problem.Contest.ContestPassword == null) || (!x.Participant.IsOfficial && x.Problem.Contest.PracticePassword == null))
                    && x.Problem.Contest.IsVisible && !x.Problem.Contest.IsDeleted);
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
    }
}
