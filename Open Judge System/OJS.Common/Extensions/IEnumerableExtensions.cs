namespace OJS.Common.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class IEnumerableExtensions
    {
        public static void Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }
    }
}
