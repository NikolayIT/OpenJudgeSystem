namespace OJS.Web.Common.Attributes
{
    using System;
    using System.Web.Mvc;
    using System.Web.Mvc.Filters;

    public class OverrideAuthorizeAttribute : AuthorizeAttribute, IOverrideFilter
    {
        public Type FiltersToOverride
        {
            get { return typeof(IAuthorizationFilter); }
        }
    }
}
