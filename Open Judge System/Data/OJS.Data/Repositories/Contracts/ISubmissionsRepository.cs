namespace OJS.Data.Repositories.Contracts
{
    using System.Linq;
    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface ISubmissionsRepository : IRepository<Submission>, IDeletableEntityRepository<Submission>
    {
        IQueryable<Submission> AllPublic();

        Submission GetSubmissionForProcessing();
    }
}
