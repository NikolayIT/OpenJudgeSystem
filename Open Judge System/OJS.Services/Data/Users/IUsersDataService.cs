namespace OJS.Services.Data.Users
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        UserProfile GetById(string userId);

        IQueryable<UserProfile> GetAll();

        IQueryable<UserProfile> GetAllWithDeleted();

        void DeleteById(string id);
    }
}