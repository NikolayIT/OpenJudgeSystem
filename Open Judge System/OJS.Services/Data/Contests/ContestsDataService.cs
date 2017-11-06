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

        public Contest GetFirstOrDefault(int id) =>
            this.contests.All().FirstOrDefault(c => c.Id == id);

        public Contest GetContestForSimpleResults(int id) =>
            this.contests
                .All()
                .Include(c => c.Problems)
                .Include(c => c.Participants.Select(par => par.User))
                .Include(c => c.Participants.Select(par => par.Scores.Select(sc => sc.Problem)))
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

        public Contest GetContestForFullResults(int id) =>
            this.contests
                .All()
                .Include(c => c.Problems)
                .Include(c => c.Participants.Select(par => par.User))
                .Include(c => c.Participants.Select(par => par.Scores.Select(sc => sc.Problem)))
                .Include(c => c.Participants.Select(par => par.Scores.Select(sc => sc.Submission.TestRuns)))
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == id);

        public bool UserHasAccessToContest(int contestId, string userId, bool isAdmin) =>
            isAdmin ||
            this.contests
                .All()
                .Any(c =>
                    c.Id == contestId &&
                    (c.Lecturers.Any(l => l.LecturerId == userId) ||
                     c.Category.Lecturers.Any(cl => cl.LecturerId == userId)));

        public IQueryable<Contest> GetByIdQuery(int id) =>
            this.contests.All().Where(c => c.Id == id);
    }
}