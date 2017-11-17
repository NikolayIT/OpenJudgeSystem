namespace OJS.Web
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    using MissingFeatures;

    public class FilterConfig
    {
        public static void RegisterGlobalFilters(
            GlobalFilterCollection filters,
            IEnumerable<object> otherFilters = null)
        {
            filters.Add(new HandleErrorAttribute());

            otherFilters?.ForEach(filters.Add);
        }
    }
}