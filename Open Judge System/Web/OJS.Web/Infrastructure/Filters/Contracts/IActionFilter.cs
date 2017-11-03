namespace OJS.Web.Infrastructure.Filters.Contracts
{
    using System;
    using System.Web.Mvc;

    public interface IActionFilter<in TAttribute>
        where TAttribute : Attribute
    {
        void OnActionExecuting(TAttribute attribute, ActionExecutingContext filterContext);

        void OnActionExecuted(TAttribute attribute, ActionExecutedContext filterContext);
    }
}