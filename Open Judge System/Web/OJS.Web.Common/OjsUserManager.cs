namespace OJS.Web.Common
{
    using Microsoft.AspNet.Identity;

    public class OjsUserManager<T> : UserManager<T>
        where T : class, IUser
    {
        public OjsUserManager(IUserStore<T> userStore)
            : base(userStore)
        {
            // changing the default user validator so that usernames can contain
            // not only alphanumeric characters
            this.UserValidator = new UserValidator<T>(this)
            {
                AllowOnlyAlphanumericUserNames = false
            };
        }
    }
}
