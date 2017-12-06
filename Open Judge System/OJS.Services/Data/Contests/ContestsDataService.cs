namespace OJS.Services.Data.Contests
{
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestsDataService : IContestsDataService
    {
        private readonly IEfGenericRepository<Contest> contests;

        public ContestsDataService(IEfGenericRepository<Contest> contests) =>
            this.contests = contests;

        public Contest GetByIdWithProblems(int contestId) =>
            this.contests.All().Include(c => c.Problems).FirstOrDefault(c => c.Id == contestId);

        public IQueryable<Contest> GetByIdQuery(int contestId) =>
            this.contests.All().Where(c => c.Id == contestId);

        public bool UserHasAccessByIdUserIdAndIsAdmin(int contestId, string userId, bool isAdmin) =>
            isAdmin ||
            this.contests
                .All()
                .Any(c =>
                    c.Id == contestId &&
                    (c.Lecturers.Any(l => l.LecturerId == userId) ||
                     c.Category.Lecturers.Any(cl => cl.LecturerId == userId)));
    }
}