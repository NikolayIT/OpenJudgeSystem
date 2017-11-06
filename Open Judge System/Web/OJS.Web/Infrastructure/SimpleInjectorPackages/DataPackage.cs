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
            container.Register<IOjsData, OjsData>(Lifestyle.Scoped);
            container.Register<IOjsDbContext, OjsDbContext>(Lifestyle.Scoped);

            container.Register(typeof(IRepository<>), typeof(EfGenericRepository<>), Lifestyle.Scoped);
            container.Register(typeof(IEfGenericRepository<>), typeof(EfGenericRepository<>), Lifestyle.Scoped);
        }
    }
}