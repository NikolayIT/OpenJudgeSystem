namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Mvc;

    using OJS.Common.Extensions;
    using OJS.Common.Models;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        private readonly SystemRole[] allRoles;

        public AuthorizeRolesAttribute(params SystemRole[] roles)
        {
            this.allRoles = roles;
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            IPrincipal user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            if (!this.allRoles.Any(x => user.IsInRole(x.GetDescription())))
            {
                return false;
            }

            return true;
        }
    }
}