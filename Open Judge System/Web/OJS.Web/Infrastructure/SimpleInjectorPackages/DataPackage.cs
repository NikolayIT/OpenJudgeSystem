namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using SimpleInjector;
    using SimpleInjector.Packaging;

    using OJS.Data;
    using OJS.Data.Contracts;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;
    
    public class DataPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<IOjsData, OjsData>();
            container.Register<IOjsDbContext, OjsDbContext>();

            container.Register(typeof(IRepository<>), typeof(EfGenericRepository<>));
            container.Register(typeof(IEfGenericRepository<>), typeof(EfGenericRepository<>));
        }
    }
}