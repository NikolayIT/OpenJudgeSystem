namespace OJS.Services.Business.Users
{
    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersBusinessService : IService
    {
        UserProfile RegisterById(string userId);
    }
}