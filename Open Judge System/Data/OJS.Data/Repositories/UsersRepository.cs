namespace OJS.Data.Repositories
{
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class UsersRepository : GenericRepository<UserProfile>, IUsersRepository
    {
        public UsersRepository(IOjsDbContext context)
            : base(context)
        {
        }

        public UserProfile GetByUsername(string username)
        {
            return this.All().FirstOrDefault(x => x.UserName == username);
        }
    }
}
