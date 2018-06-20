namespace OJS.Services.Data.ExamGroups
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class ExamGroupsDataService : IExamGroupsDataService
    {
        private readonly IEfGenericRepository<ExamGroup> examGroups;

        public ExamGroupsDataService(IEfGenericRepository<ExamGroup> examGroups) =>
            this.examGroups = examGroups;

        public void Add(ExamGroup examGroup)
        {
            this.examGroups.Add(examGroup);
            this.examGroups.SaveChanges();
        }

        public void Update(ExamGroup examGroup)
        {
            this.examGroups.Update(examGroup);
            this.examGroups.SaveChanges();
        }

        public ExamGroup GetById(int id) => this.examGroups.GetById(id);

        public ExamGroup GetByExternalIdAndAppId(int? externalId, string appId) =>
            this.examGroups
                .All()
                .FirstOrDefault(eg =>
                    eg.ExternalExamGroupId == externalId &&
                    eg.ExternalAppId == appId);

        public int GetIdByExternalIdAndAppId(int? externalId, string appId) =>
            this.examGroups
                .All()
                .Where(eg => eg.ExternalExamGroupId == externalId && eg.ExternalAppId == appId)
                .Select(eg => eg.Id)
                .FirstOrDefault();

        public int? GetContestIdById(int id) =>
            this.GetByIdQuery(id)
                .Select(eg => eg.ContestId)
                .FirstOrDefault();

        public IQueryable<ExamGroup> GetAll() => this.examGroups.All();

        public IQueryable<ExamGroup> GetAllByLecturer(string lecturerId) =>
            this.GetAll()
                .Where(eg =>
                    eg.Contest == null ||
                    (eg.Contest.Lecturers.Any(l => l.LecturerId == lecturerId) ||
                    eg.Contest.Category.Lecturers.Any(l => l.LecturerId == lecturerId)));

        public IQueryable<UserProfile> GetUsersByIdQuery(int id) =>
            this.GetByIdQuery(id).SelectMany(eg => eg.Users);

        public IQueryable<ExamGroup> GetByIdQuery(int id) =>
            this.examGroups.All().Where(eg => eg.Id == id);

        public void RemoveUserByIdAndUser(int id, string userId)
        {
            var examGroup = this.GetById(id);
            var user = examGroup?.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                examGroup.Users.Remove(user);
                this.examGroups.SaveChanges();
            }
        }

        public void RemoveContestByContest(int contestId) =>
            this.examGroups.Update(
                eg => eg.ContestId == contestId,
                examGroup => new ExamGroup
                {
                    ContestId = null
                });
    }
}