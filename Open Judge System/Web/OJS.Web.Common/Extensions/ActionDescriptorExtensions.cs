namespace OJS.Web.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    public static class ActionDescriptorExtensions
    {
        public static IEnumerable<Attribute> GetAppliedCustomAttributes(this ActionDescriptor actionDescriptor)
        {
            var attributes = actionDescriptor.ControllerDescriptor
                .GetCustomAttributes(true)
                .Concat(actionDescriptor.GetCustomAttributes(true))
                .Cast<Attribute>();

            return attributes;
        }
    }
}