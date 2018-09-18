namespace OJS.Services.Data.Contests
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

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
            this.GetAll()
                .Where(c => c.Id == id);

        public IQueryable<Contest> GetAll() => this.contests.All();

        public IQueryable<Contest> GetAllActive() =>
            this.GetAllVisible()
                .Where(c =>
                    c.StartTime <= DateTime.Now &&
                    (c.EndTime >= DateTime.Now ||
                        (c.Type == ContestType.OnlinePracticalExam && c.Participants.Any(p =>
                            p.IsOfficial &&
                            p.ParticipationEndTime >= DateTime.Now))));

        public IQueryable<Contest> GetAllCompetable() =>
            this.GetAllVisible()
                .Where(c =>
                    c.StartTime <= DateTime.Now &&
                    c.EndTime.HasValue &&
                    c.EndTime >= DateTime.Now);

        public IQueryable<Contest> GetAllInactive() =>
            this.GetAll()
                .Where(c =>
                    c.StartTime > DateTime.Now ||
                    (c.EndTime < DateTime.Now && c.Type != ContestType.OnlinePracticalExam) ||
                    !c.Participants.Any(p => p.ParticipationEndTime < DateTime.Now));

        public IQueryable<Contest> GetAllUpcoming() =>
            this.GetAllVisible()
                .Where(c => c.StartTime > DateTime.Now);

        public IQueryable<Contest> GetAllPast() =>
            this.GetAllVisible()
                .Where(c => c.EndTime < DateTime.Now);

        public IQueryable<Contest> GetAllVisible() =>
            this.GetAll()
                .Where(c => c.IsVisible);

        public IQueryable<Contest> GetAllVisibleByCategory(int categoryId) =>
            this.GetAllVisible()
                .Where(c => c.CategoryId == categoryId);

        public IQueryable<Contest> GetAllVisibleBySubmissionType(int submissionTypeId) =>
            this.GetAllVisible()
                .Where(c => c.ProblemGroups
                    .SelectMany(pg => pg.Problems)
                    .Any(p => p.SubmissionTypes.Any(s => s.Id == submissionTypeId)));

        public IQueryable<Contest> GetAllByLecturer(string lecturerId) =>
            this.GetAll()
                .Where(c =>
                    c.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    c.Category.Lecturers.Any(l => l.LecturerId == lecturerId));

        public IQueryable<Contest> GetAllVisibleByCategoryAndLecturer(int categoryId, string lecturerId) =>
            this.GetAllByLecturer(lecturerId)
                .Where(c => c.CategoryId == categoryId);

        public IQueryable<Contest> GetAllWithDeleted() => this.contests.AllWithDeleted();

        public int GetMaxPointsById(int id) =>
            this.GetMaxPointsByIdAndProblemGroupsFilter(id, pg => true);

        public int GetMaxPointsForExportById(int id) =>
            this.GetMaxPointsByIdAndProblemGroupsFilter(id, pg => pg.Type != ProblemGroupType.ExcludedFromHomework);

        public string GetNameById(int id) =>
            this.GetByIdQuery(id)
                .Select(c => c.Name)
                .FirstOrDefault();

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
            this.GetByIdQuery(id)
                .Any(c =>
                    c.Lecturers.Any(l => l.LecturerId == userId) ||
                    c.Category.Lecturers.Any(l => l.LecturerId == userId));

        public bool IsUserParticipantInByContestAndUser(int id, string userId) =>
            this.GetAll()
                .Any(c =>
                    c.Id == id &&
                    c.Participants.Any(p => p.UserId == userId));

        public bool IsUserInExamGroupByContestAndUser(int id, string userId) =>
            this.GetAll()
                .Any(c =>
                    c.Id == id &&
                    c.ExamGroups.Any(eg => eg.Users.Any(u => u.Id == userId)));

        public void Add(Contest contest)
        {
            this.contests.Add(contest);
            this.contests.SaveChanges();
        }

        public void Update(Contest contest)
        {
            this.contests.Update(contest);
            this.contests.SaveChanges();
        }

        private int GetMaxPointsByIdAndProblemGroupsFilter(int id, Expression<Func<ProblemGroup, bool>> filter) =>
            this.GetByIdQuery(id)
                .Select(c => c.ProblemGroups
                    .AsQueryable()
                    .Where(pg => pg.Problems.Any(p => !p.IsDeleted))
                    .Where(filter)
                    .Sum(pg => (int?)pg.Problems.FirstOrDefault().MaximumPoints))
                .FirstOrDefault() ?? default(int);
    }
}