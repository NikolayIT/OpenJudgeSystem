namespace OJS.Web.Infrastructure.Filters.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class ClearContestCategoryCacheAttribute : Attribute
    {
        public ClearContestCategoryCacheAttribute(string queryKeyForCategoryId) =>
            this.QuerKeyForCategoryId = queryKeyForCategoryId;

        public string QuerKeyForCategoryId { get; }
    }
}