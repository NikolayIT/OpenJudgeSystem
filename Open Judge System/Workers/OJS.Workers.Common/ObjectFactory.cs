namespace OJS.Workers.Common
{
    using System;

    using Extensions;

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