namespace OJS.Data.Repositories.Contracts
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Contracts;
    using OJS.Data.Models;

    public interface IProblemsRepository : IRepository<Problem>, IDeletableEntityRepository<Problem>
    {
        IQueryable<Problem> AllIncludesSubmissionsTestsAndResources(Expression<Func<Problem, bool>> orExpressionProblemIds);
    }
}
