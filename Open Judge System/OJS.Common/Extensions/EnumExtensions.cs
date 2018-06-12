namespace OJS.Common.Extensions
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using OJS.Common.Attributes;

    public static class EnumExtensions
    {
        public static string GetDisplayName<T>(this T enumerationValue)
        {
            return GetDisplayName(enumerationValue, typeof(DisplayAttribute));
        }

        /// <summary>
        /// Extends the enumeration so that if it has Description attribute on top of the value, it can be taken as a friendly text instead of the basic ToString method
        /// </summary>
        /// <typeparam name="T">Type of the enumeration.</typeparam>
        /// <returns>Enum description</returns>
        public static string GetDescription<T>(this T enumerationValue)
            where T : struct
        {
            return GetEnumDescription(enumerationValue, typeof(DescriptionAttribute));
        }

        public static string GetLocalizedDescription<T>(this T enumerationValue)
            where T : struct
        {
            return GetEnumDescription(enumerationValue, typeof(LocalizedDescriptionAttribute));
        }

        public static T? GetValidTypeOrNull<T>(this T? enumerationValue)
            where T : struct
        {
            if (enumerationValue.HasValue &&
                Enum.IsDefined(typeof(T), enumerationValue) &&
                !enumerationValue.Value.Equals(default(T)))
            {
                return enumerationValue;
            }

            return null;
        }

        private static string GetEnumDescription<T>(this T enumerationValue, Type descriptionType)
        {
            var type = enumerationValue.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException("EnumerationValue must be of Enum type", nameof(enumerationValue));
            }

            // Tries to find a DescriptionAttribute for a potential friendly name for the enum
            var memberInfo = type.GetMember(enumerationValue.ToString());
            if (memberInfo.Length > 0)
            {
                var customAttributes = memberInfo[0].GetCustomAttributes(descriptionType, false);

                if (customAttributes.Length > 0)
                {
                    // Pull out the description value
                    return ((DescriptionAttribute)customAttributes[0]).Description;
                }
            }

            // If we have no description attribute, just return the ToString of the enum
            return enumerationValue.ToString();
        }

        private static string GetDisplayName<T>(this T value, Type descriptionType)
        {
            var type = value.GetType();

            var memberInfo = type.GetMember(value.ToString());
            if (memberInfo.Length > 0)
            {
                var customAttributes = memberInfo[0].GetCustomAttributes(descriptionType, false);

                if (customAttributes.Length > 0)
                {
                    // Pull out the name value
                    return ((DisplayAttribute)customAttributes[0]).Name;
                }
            }

            return value.ToString();
        }
    }
}
