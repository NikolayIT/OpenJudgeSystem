namespace OJS.Common.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public class LocalizedDisplayFormatAttribute : DisplayFormatAttribute
    {
        public LocalizedDisplayFormatAttribute(string resourceName, Type resourceType = null)
        {
            if (resourceType != null)
            {
                var resourceProperty = resourceType.GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public);
                if (resourceProperty != null)
                {
                    this.NullDisplayText = (string)resourceProperty.GetValue(resourceProperty.DeclaringType, null);
                }
            }
        }
    }
}
