namespace OJS.Data.Contracts.DataAnnotations
{
    using System;

    public class IsUnicodeAttribute : Attribute
    {
        public IsUnicodeAttribute(bool isUnicode)
        {
            this.IsUnicode = isUnicode;
        }

        public bool IsUnicode { get; }
    }
}
