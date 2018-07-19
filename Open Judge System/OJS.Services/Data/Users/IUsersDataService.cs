namespace OJS.Services.Data.Users
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        UserProfile GetByIdIncludingDeleted(string userId);

        UserProfile GetById(string userId);

        UserProfile GetByUsernameIncludingDeleted(string username);

        IQueryable<UserProfile> GetAll();

        void DeleteById(string id);
    }
}