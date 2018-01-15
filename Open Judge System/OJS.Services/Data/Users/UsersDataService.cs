namespace OJS.Services.Data.Users
{
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Contracts;

    public class UsersDataService : IUsersDataService
    {
        private readonly IEfDeletableEntityRepository<UserProfile> users;

        public UsersDataService(IEfDeletableEntityRepository<UserProfile> users) =>
            this.users = users;

        public async Task<UserProfile> GetByIdIncludingDeletedAsync(string userId) =>
            await this.users.AllWithDeleted().FirstOrDefaultAsync(u => u.Id == userId);

        public UserProfile GetById(string id) =>
            this.users.All().FirstOrDefault(u => u.Id == id);
    }
}