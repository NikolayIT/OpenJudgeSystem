namespace OJS.Services.Data.Users
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class UsersDataService : IUsersDataService
    {
        private readonly IEfDeletableEntityRepository<UserProfile> users;

        public UsersDataService(IEfDeletableEntityRepository<UserProfile> users) =>
            this.users = users;

        public UserProfile GetByUserIdIncludingDeleted(string userId) =>
            this.users.AllWithDeleted().SingleOrDefault(u => u.Id == userId);
    }
}