namespace OJS.Common.Attributes
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string resourceName, Type resourceType = null)
        {
            var resourceProperty = resourceType?.GetProperty(resourceName, BindingFlags.Static | BindingFlags.Public);
            if (resourceProperty != null)
            {
                this.DescriptionValue = (string)resourceProperty.GetValue(resourceProperty.DeclaringType, null);
            }
        }
    }
}
