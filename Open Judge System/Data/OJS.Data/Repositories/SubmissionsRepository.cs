namespace OJS.Data.Repositories
{
    using System;
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
    }
}
