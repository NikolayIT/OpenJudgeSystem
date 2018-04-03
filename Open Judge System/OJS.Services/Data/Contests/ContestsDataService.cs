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

        public Contest GetById(int id) => this.contests.GetById(id);

        public Contest GetByIdWithProblems(int id) =>
            this.GetAll()
                .Include(c => c.ProblemGroups.Select(pg => pg.Problems))
                .FirstOrDefault(c => c.Id == id);

        public IQueryable<Contest> GetByIdQuery(int id) =>
            this.GetAll().Where(c => c.Id == id);

        public IQueryable<Contest> GetAll() => this.contests.All();

        public IQueryable<Contest> GetAllActive() =>
            this.GetAll().Where(c =>
                c.IsVisible && c.StartTime <= DateTime.Now &&
                (c.EndTime >= DateTime.Now ||
                    c.Type == ContestType.OnlinePracticalExam &&
                    c.Participants.Any(p => p.IsOfficial && p.ParticipationEndTime >= DateTime.Now)));

        public IQueryable<Contest> GetAllCompetable() =>
            this.GetAll().Where(c =>
                c.IsVisible &&
                c.StartTime <= DateTime.Now &&
                c.EndTime.HasValue &&
                c.EndTime >= DateTime.Now);

        public IQueryable<Contest> GetAllInactive() =>
            this.GetAll().Where(c =>
                c.StartTime > DateTime.Now ||
                (c.EndTime < DateTime.Now && c.Type != ContestType.OnlinePracticalExam) ||
                    !c.Participants.Any(p => p.ParticipationEndTime < DateTime.Now));

        public IQueryable<Contest> GetAllUpcoming() =>
            this.GetAll().Where(c => c.StartTime > DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllPast() =>
            this.GetAll().Where(c => c.EndTime < DateTime.Now && c.IsVisible);

        public IQueryable<Contest> GetAllVisible() => this.GetAll().Where(c => c.IsVisible);

        public IQueryable<Contest> GetAllVisibleByCategory(int categoryId) =>
            this.GetAllVisible().Where(c => c.CategoryId == categoryId);

        public IQueryable<Contest> GetAllVisibleByLecturer(string lecturerId) =>
            this.GetAllVisible().Where(c =>
                c.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                c.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public IQueryable<Contest> GetAllVisibleByCategoryAndLecturer(int categoryId, string lecturerId) =>
            this.GetAllVisibleByLecturer(lecturerId).Where(c => c.CategoryId == categoryId);

        public IQueryable<Contest> GetAllWithDeleted() => this.contests.AllWithDeleted();

        public int GetIdById(int id) =>
            this.GetByIdQuery(id).Select(c => c.Id).SingleOrDefault();

        public bool IsActiveById(int id)
        {
            var contest = this.contests.GetById(id);
            return contest != null && contest.IsActive;
        }

        public bool IsOnlineById(int id) =>
            this.GetByIdQuery(id)
                .Select(c => c.Type)
                .FirstOrDefault() == ContestType.OnlinePracticalExam;

        public bool ExistsById(int id) => this.GetAll().Any(c => c.Id == id);

        public bool IsUserLecturerInByContestAndUser(int id, string userId) =>
            this.GetByIdQuery(id).Any(c => c.Lecturers.Any(l => l.LecturerId == userId) ||
                c.Category.Lecturers.Any(l => l.LecturerId == userId));

        public bool IsUserParticipantInByContestAndUser(int id, string userId) =>
            this.GetAll().Any(c => c.Id == id && c.Participants.Any(p => p.UserId == userId));

        public bool IsUserInExamGroupByContestAndUser(int id, string userId) =>
            this.GetAll().Any(c => c.Id == id &&
                c.ExamGroups.Any(eg => eg.Users.Any(u => u.Id == userId)));
    }
}