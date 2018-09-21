namespace OJS.Services.Data.Contests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetById(int id);

        Contest GetByIdWithProblems(int id);

        IQueryable<Contest> GetByIdQuery(int id);

        IQueryable<Contest> GetAll();

        IQueryable<Contest> GetAllActive();

        IQueryable<Contest> GetAllCompetable();

        IQueryable<Contest> GetAllInactive();

        IQueryable<Contest> GetAllUpcoming();

        IQueryable<Contest> GetAllPast();

        IQueryable<Contest> GetAllVisible();

        IQueryable<Contest> GetAllVisibleByCategory(int categoryId);

        IQueryable<Contest> GetAllVisibleBySubmissionType(int submissionTypeId);

        IQueryable<Contest> GetAllByLecturer(string lecturerId);

        IQueryable<Contest> GetAllVisibleByCategoryAndLecturer(int categoryId, string lecturerId);

        IQueryable<Contest> GetAllWithDeleted();

        int GetMaxPointsById(int id);

        int GetMaxPointsForExportById(int id);

        string GetNameById(int id);

        bool IsActiveById(int id);

        bool IsOnlineById(int id);

        bool ExistsById(int id);

        bool IsUserLecturerInByContestAndUser(int id, string userId);

        bool IsUserParticipantInByContestAndUser(int id, string userId);

        bool IsUserInExamGroupByContestAndUser(int id, string userId);

        void Add(Contest contest);

        void Update(Contest contest);
    }
}