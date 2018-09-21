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

        public UserProfile GetByUsername(string username) =>
            this.GetAll()
                .FirstOrDefault(u => u.UserName == username);

        public IQueryable<UserProfile> GetAll() =>
            this.users.All();

        public IQueryable<UserProfile> GetAllWithDeleted() =>
            this.users.AllWithDeleted();

        public IQueryable<UserProfile> GetAllByRole(string roleId) =>
            this.GetAll()
                .Where(x => x.Roles.Any(y => y.RoleId == roleId));

        public void DeleteById(string id) =>
            this.users.Delete(u => u.Id == id);
    }
}