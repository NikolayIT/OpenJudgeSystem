namespace OJS.Data.Repositories
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class TestRepository : GenericRepository<Test>, ITestRepository
    {
        public TestRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public override void Delete(int id)
        {
            // TODO: Evaluate if this is the best solution
            var test = this.Context.DbContext.ChangeTracker.Entries<Test>().FirstOrDefault(t => t.Property(pr => pr.Id).CurrentValue == id).Entity ?? new Test { Id = id };
            this.Context.Entry(test).State = EntityState.Deleted;
        }
    }
}
