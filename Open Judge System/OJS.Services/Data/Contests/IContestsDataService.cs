namespace OJS.Services.Data.Contests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetById(int id);

        IQueryable<Contest> GetByIdQuery(int id);

        IQueryable<Contest> GetByCategory(int categoryId);

        IQueryable<Contest> GetAll();

        IQueryable<Contest> GetAllActive();

        IQueryable<Contest> GetAllCompetable();

        IQueryable<Contest> GetAllInactive();

        IQueryable<Contest> GetAllUpcoming();

        IQueryable<Contest> GetAllPast();

        IQueryable<Contest> GetAllVisible();

        IQueryable<Contest> GetAllWithDeleted();

        IQueryable<Contest> GetAllByLecturer(string lecturerId);

        int GetIdById(int id);

        void DeleteById(int id);

        bool IsActiveById(int id);

        bool ExistsById(int id);

        bool IsUserLecturerInByContestAndUser(int id, string userId);

        bool IsUserParticipantInByContestAndUser(int id, string userId);

        bool IsUserInExamGroupByContestAndUser(int id, string userId);
    }
}