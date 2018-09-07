namespace OJS.Services.Data.Users
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        UserProfile GetById(string userId);

        UserProfile GetByUsername(string username);

        IQueryable<UserProfile> GetAll();

        IQueryable<UserProfile> GetAllWithDeleted();

        IQueryable<UserProfile> GetAllByRole(string roleId);

        void DeleteById(string id);
    }
}