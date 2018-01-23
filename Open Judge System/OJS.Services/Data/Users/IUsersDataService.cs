namespace OJS.Services.Data.Users
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        UserProfile GetByIdIncludingDeleted(string userId);

        UserProfile GetById(string userId);

        IQueryable<UserProfile> GetAll();
    }
}