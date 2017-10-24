namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using OJS.Data;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class DataPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<IOjsData, OjsData>();
            container.Register<IOjsDbContext, OjsDbContext>();
        }
    }
}