namespace OJS.Data.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class ProblemsRepository : DeletableEntityRepository<Problem>, IProblemsRepository
    {
        public ProblemsRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public IQueryable<Problem> AllIncludesSubmissionsTestsAndResources(Expression<Func<Problem, bool>> orExpressionProblemIds)
        {
            return this.All().Where(orExpressionProblemIds).Include(p => p.Submissions).Include(p => p.Tests).Include(p => p.Resources);
        }
    }
}