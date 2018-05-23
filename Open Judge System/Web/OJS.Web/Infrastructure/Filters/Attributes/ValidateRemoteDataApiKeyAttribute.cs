namespace OJS.Web.Infrastructure.Filters.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ValidateRemoteDataApiKeyAttribute : Attribute
    {
    }
}