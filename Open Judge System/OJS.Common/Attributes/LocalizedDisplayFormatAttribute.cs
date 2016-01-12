namespace OJS.Common.Attributes
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Reflection;

    public class LocalizedDisplayFormatAttribute : DisplayFormatAttribute
    {
        public LocalizedDisplayFormatAttribute()
        {
            var resourceProperty = this.NullDisplayTextResourceType?.GetProperty(this.NullDisplayTextResourceName, BindingFlags.Static | BindingFlags.Public);
            if (resourceProperty != null)
            {
                this.NullDisplayText = (string)resourceProperty.GetValue(resourceProperty.DeclaringType, null);
            }
        }

        public Type NullDisplayTextResourceType { get; set; }

        public string NullDisplayTextResourceName { get; set; }
    }
}
