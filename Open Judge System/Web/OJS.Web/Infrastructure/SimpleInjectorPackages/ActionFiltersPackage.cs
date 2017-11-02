namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using SimpleInjector;
    using SimpleInjector.Packaging;

    using OJS.Web.Infrastructure.Filters.Contracts;

    public class ActionFiltersPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.RegisterCollection(typeof(IActionFilter<>), typeof(IActionFilter<>).Assembly);
        }
    }
}