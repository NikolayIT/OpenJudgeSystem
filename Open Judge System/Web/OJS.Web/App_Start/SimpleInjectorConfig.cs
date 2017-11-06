[assembly: System.Web.PreApplicationStartMethod(typeof(OJS.Web.SimpleInjectorConfig), "Initialize")]

namespace OJS.Web
{
    using System.Reflection;
    using System.Web.Mvc;

    using SimpleInjector;
    using SimpleInjector.Integration.Web;
    using SimpleInjector.Integration.Web.Mvc;
    using SimpleInjector.Lifestyles;

    public static class SimpleInjectorConfig
    {
        public static Container Container { get; private set; }

        public static void Initialize()
        {
            var container = BuildContainer();

            DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));

            Container = container;
        }

        private static Container BuildContainer()
        {
            var container = new Container();
            var assembly = Assembly.GetExecutingAssembly();

            container.Options.DefaultScopedLifestyle = Lifestyle.CreateHybrid(
                new WebRequestLifestyle(),
                new AsyncScopedLifestyle());

            container.RegisterPackages(new[] { assembly });
            container.RegisterMvcControllers(assembly);

            container.Verify();

            return container;
        }
    }
}