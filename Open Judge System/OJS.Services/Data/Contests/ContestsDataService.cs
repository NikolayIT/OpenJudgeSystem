namespace OJS.Services.Data.Contests
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestsDataService : IContestsDataService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;

        public ContestsDataService(IEfDeletableEntityRepository<Contest> contests) =>
            this.contests = contests;

        public Contest GetById(int contestId) => this.contests.GetById(contestId);

        public Contest GetByIdWithProblems(int contestId) =>
            this.contests.All().Include(c => c.Problems).FirstOrDefault(c => c.Id == contestId);

        public IQueryable<Contest> GetByIdQuery(int contestId) =>
            this.contests.All().Where(c => c.Id == contestId);

        public IQueryable<Contest> GetAllActive() =>
            this.contests
                .All()
                .Where(c => c.IsVisible && c.StartTime <= DateTime.Now &&
                    (c.EndTime >= DateTime.Now ||
                        c.Type == ContestType.OnlinePracticalExam &&
                        c.Participants.Any(p => p.IsOfficial && p.ContestEndTime >= DateTime.Now)));

        public IQueryable<Contest> GetAllCompetable() =>
            this.contests
                .All()
                .Where(c => c.IsVisible &&
                    c.StartTime <= DateTime.Now &&
                    c.EndTime.HasValue &&
                    c.EndTime >= DateTime.Now);

        public IQueryable<Contest> GetAllInactive() =>
            this.contests
            .All()
            .Where(c => c.StartTime > DateTime.Now ||
                (c.EndTime < DateTime.Now && c.Type != ContestType.OnlinePracticalExam) ||
                    !c.Participants.Any(p => p.ContestEndTime < DateTime.Now));

        public IQueryable<Contest> GetAllUpcoming() =>
            this.contests.All().Where(c => c.StartTime > DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllPast() =>
            this.contests.All().Where(c => c.EndTime < DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllVisible() => this.contests.All().Where(c => c.IsVisible);

        public IQueryable<Contest> GetAllWithDeleted() => this.contests.AllWithDeleted();

        public void DeleteById(int contestId)
        {
            this.contests.Delete(contestId);
            this.contests.SaveChanges();
        }

        public bool IsActiveById(int contestId)
        {
            var contest = this.contests.GetById(contestId);
            return contest != null && contest.IsActive;
        }

        public bool IsUserLecturerInByContestAndUser(int contestId, string userId) =>
            this.contests
                .All()
                .Where(c => c.Id == contestId)
                .Any(c => c.Lecturers.Any(l => l.LecturerId == userId) ||
                    c.Category.Lecturers.Any(l => l.LecturerId == userId));

        public bool IsUserParticipantInByContestAndUser(int contestId, string userId) =>
            this.contests
                .All()
                .Any(c => c.Id == contestId && c.Participants.Any(p => p.UserId == userId));
    }
}