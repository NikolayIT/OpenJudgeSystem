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

        public UserProfile GetByIdIncludingDeleted(string userId) =>
            this.users
                .AllWithDeleted()
                .FirstOrDefault(u => u.Id == userId);

        public UserProfile GetById(string id) =>
            this.GetAll()
                .FirstOrDefault(u => u.Id == id);

        public IQueryable<UserProfile> GetAll() => this.users.All();
    }
}