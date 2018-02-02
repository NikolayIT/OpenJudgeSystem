namespace OJS.Services.Data.Contests
{
    using System;
    using System.Linq;

    using OJS.Common.Models;
    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ContestsDataService : IContestsDataService
    {
        private readonly IEfDeletableEntityRepository<Contest> contests;

        public ContestsDataService(IEfDeletableEntityRepository<Contest> contests) =>
            this.contests = contests;

        public Contest GetById(int id) => this.contests.GetById(id);

        public IQueryable<Contest> GetByIdQuery(int contestId) =>
            this.contests.All().Where(c => c.Id == contestId);

        public IQueryable<Contest> GetAll() => this.contests.All();

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

        public IQueryable<Contest> GetAllByLecturer(string lecturerId) =>
            this.GetAll().Where(c =>
                c.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                c.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public int GetIdById(int id) =>
            this.GetAll()
                .Where(c => c.Id == id)
                .Select(c => c.Id)
                .SingleOrDefault();

        public void DeleteById(int id)
        {
            this.contests.Delete(id);
            this.contests.SaveChanges();
        }

        public bool IsActiveById(int id)
        {
            var contest = this.contests.GetById(id);
            return contest != null && contest.IsActive;
        }

        public bool ExistsById(int id) => this.GetAll().Any(c => c.Id == id);

        public bool IsUserLecturerInByContestAndUser(int id, string userId) =>
            this.contests
                .All()
                .Where(c => c.Id == id)
                .Any(c => c.Lecturers.Any(l => l.LecturerId == userId) ||
                    c.Category.Lecturers.Any(l => l.LecturerId == userId));

        public bool IsUserParticipantInByContestAndUser(int id, string userId) =>
            this.contests
                .All()
                .Any(c => c.Id == id && c.Participants.Any(p => p.UserId == userId));

        public bool IsUserInExamGroupByContestAndUser(int id, string userId) =>
            this.contests
                .All()
                .Any(c => c.Id == id &&
                    c.ExamGroups.Any(eg => eg.Users.Any(u => u.Id == userId)));
    }
}