namespace OJS.Web.Infrastructure.Filters
{
    using System;
    using System.Collections;
    using System.Web.Mvc;

    using OJS.Web.Common.Extensions;
    using OJS.Web.Infrastructure.Filters.Contracts;

    public sealed class ActionFilterDispatcher : IActionFilter
    {
        private readonly Func<Type, IEnumerable> container;

        public ActionFilterDispatcher(Func<Type, IEnumerable> container) => this.container = container;

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var attributes = filterContext.ActionDescriptor.GetAppliedCustomAttributes();

            foreach (var attribute in attributes)
            {
                var filters = this.GetActionFilters(attribute);

                foreach (dynamic actionFilter in filters)
                {
                    actionFilter.OnActionExecuting((dynamic)attribute, filterContext);
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var attributes = filterContext.ActionDescriptor.GetAppliedCustomAttributes();

            foreach (var attribute in attributes)
            {
                var filters = this.GetActionFilters(attribute);

                foreach (dynamic actionFilter in filters)
                {
                    actionFilter.OnActionExecuted((dynamic)attribute, filterContext);
                }
            }
        }

        private IEnumerable GetActionFilters(Attribute attribute)
        {
            var filterType = typeof(IActionFilter<>).MakeGenericType(attribute.GetType());

            var filters = this.container.Invoke(filterType);

            return filters;
        }
    }
}