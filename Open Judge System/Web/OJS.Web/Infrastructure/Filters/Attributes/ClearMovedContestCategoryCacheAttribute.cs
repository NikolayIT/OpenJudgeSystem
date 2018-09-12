namespace OJS.Web.Infrastructure.Filters.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class ClearMovedContestCategoryCacheAttribute : Attribute
    {
        public ClearMovedContestCategoryCacheAttribute(
            string movedCategoryIdParamName,
            string movedToCategoryIdParamName)
        {
            this.MovedCategoryIdParamName = movedCategoryIdParamName;
            this.MovedToCategoryIdParamName = movedToCategoryIdParamName;
        }

        public string MovedCategoryIdParamName { get; }

        public string MovedToCategoryIdParamName { get; }
    }
}