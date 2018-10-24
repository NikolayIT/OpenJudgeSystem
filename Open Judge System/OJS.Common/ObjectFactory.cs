namespace OJS.Common
{
    using System;

    using OJS.Common.Extensions;

    public static class ObjectFactory
    {
        private static IServiceProvider serviceProvider;

        public static void InitializeServiceProvider(IServiceProvider appServiceProvider)
        {
            serviceProvider = appServiceProvider;
        }

        public static T GetInstance<T>() => serviceProvider.GetService<T>();
    }
}