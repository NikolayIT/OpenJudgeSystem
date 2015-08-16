namespace OJS.Web.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;

    using OJS.Common.Extensions;

    public static class EnumConverter
    {
        // Convert enums to SelectListItem (used in MVC and Kendo UI dropdowns)
        public static IEnumerable<SelectListItem> GetSelectListItems<T>()
            where T : struct, IConvertible
        {
            Type type = typeof(T);
            if (!type.IsEnum)
            {
                throw new ArgumentException("T must be of Enum type");
            }

            var allTypes = Enum.GetValues(type).Cast<T>().Select(v => new SelectListItem
            {
                Text = v.GetLocalizedDescription(),
                Value = Convert.ChangeType(v, typeof(int)).ToString()
            });

            return allTypes;
        }
    }
}
