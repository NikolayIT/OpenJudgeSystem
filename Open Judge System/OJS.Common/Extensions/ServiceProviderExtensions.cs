namespace OJS.Common.Extensions
{
    using System;

    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var type = typeof(T);
            var instance = serviceProvider.GetService(type);

            if (instance == null)
            {
                throw new Exception($"Type '{type.FullName}' is not registered in the service provider.");
            }

            return (T)instance;
        }
    }
}