namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using OJS.Data;
    using OJS.Data.Archives;
    using OJS.Data.Contracts;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class DataPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            container.Register<IOjsData, OjsData>(Lifestyle.Scoped);
            container.Register<IOjsDbContext, OjsDbContext>(Lifestyle.Scoped);
            container.Register<IArchivesDbContext, ArchivesDbContext>(Lifestyle.Scoped);

            container.Register(typeof(IRepository<>), typeof(EfGenericRepository<>), Lifestyle.Scoped);
            container.Register(typeof(IEfGenericRepository<>), typeof(EfGenericRepository<>), Lifestyle.Scoped);
            container.Register(
                typeof(IEfDeletableEntityRepository<>),
                typeof(EfDeletableEntityRepository<>),
                Lifestyle.Scoped);
        }
    }
}