[assembly: System.Web.PreApplicationStartMethod(typeof(OJS.Web.SimpleInjectorConfig), "Initialize")]

namespace OJS.Web
{
    using System.Reflection;
    using System.Web.Mvc;

    using SimpleInjector;
    using SimpleInjector.Integration.Web;
    using SimpleInjector.Integration.Web.Mvc;

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

            container.Options.DefaultLifestyle = new WebRequestLifestyle();

            container.RegisterPackages(new[] { Assembly.GetExecutingAssembly() });

            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());

            container.RegisterMvcIntegratedFilterProvider();

            container.Verify();

            return container;
        }
    }
}