namespace OJS.Web.Common
{
    using System.Security.Principal;

    public static class PrincipalExtensions
    {
        public static bool IsLoggedIn(this IPrincipal principal)
        {
            return principal.Identity.IsAuthenticated;
        }

        public static bool IsAdmin(this IPrincipal principal)
        {
            return principal.IsInRole("Administrator");
        }
    }
}
