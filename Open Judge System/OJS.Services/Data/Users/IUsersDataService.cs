namespace OJS.Services.Data.Users
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        UserProfile GetByUserIdIncludingDeleted(string userId);
    }
}