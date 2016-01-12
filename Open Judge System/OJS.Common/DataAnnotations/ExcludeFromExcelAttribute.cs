namespace OJS.Common.DataAnnotations
{
    using System;

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ExcludeFromExcelAttribute : Attribute
    {
    }
}
