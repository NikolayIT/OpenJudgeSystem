namespace OJS.Data.Repositories
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    using OJS.Data.Models;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    public class UsersRepository : EfGenericRepository<UserProfile>, IUsersRepository
    {
        public UsersRepository(DbContext context)
            : base(context)
        {
        }

        public UserProfile GetByUsername(string username)
        {
            return this.All().FirstOrDefault(x => x.UserName == username);
        }

        public UserProfile GetById(string id)
        {
            return this.All().FirstOrDefault(x => x.Id == id);
        }

        public override void Delete(UserProfile entity)
        {
            throw new NotImplementedException();
        }
    }
}
