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

        public UserProfile GetById(string id) =>
            this.GetAll()
                .FirstOrDefault(u => u.Id == id);

        public IQueryable<UserProfile> GetAll() =>
            this.users.All();

        public IQueryable<UserProfile> GetAllWithDeleted() =>
            this.users.AllWithDeleted();

        public void DeleteById(string id) =>
            this.users.Delete(u => u.Id == id);
    }
}