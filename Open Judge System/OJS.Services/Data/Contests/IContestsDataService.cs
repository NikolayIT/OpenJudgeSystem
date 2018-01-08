namespace OJS.Services.Data.Contests
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IContestsDataService : IService
    {
        Contest GetById(int contestId);

        OJS.Data.Models.Contest GetByIdWithProblems(int contestId);

        IQueryable<Contest> GetByIdQuery(int contestId);

        IQueryable<Contest> GetAllActive();

        IQueryable<Contest> GetAllCompetable();

        IQueryable<Contest> GetAllInactive();

        IQueryable<Contest> GetAllUpcoming();

        IQueryable<Contest> GetAllPast();

        IQueryable<Contest> GetAllVisible();

        IQueryable<Contest> GetAllWithDeleted();

        void DeleteById(int contestId);

        bool IsActiveById(int contestId);

        bool IsUserLecturerInByContestAndUser(int contestId, string userId);

        bool IsUserParticipantInByContestAndUser(int contestId, string userId);
    }
}