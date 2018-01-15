namespace OJS.Services.Data.Users
{
    using System.Threading.Tasks;

    using OJS.Data.Models;
    using OJS.Services.Common;

    public interface IUsersDataService : IService
    {
        Task<UserProfile> GetByIdIncludingDeletedAsync(string userId);
    }
}