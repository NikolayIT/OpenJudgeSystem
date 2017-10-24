namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using OJS.Data;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class DataPackages : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<IOjsData, OjsData>();
            container.Register<IOjsDbContext, OjsDbContext>();
        }
    }
}