namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Data.Entity;

    using OJS.Data;
    using OJS.Data.Archives;
    using OJS.Data.Archives.Repositories;
    using OJS.Data.Archives.Repositories.Contracts;
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

            container.Register<OjsDbContext>(Lifestyle.Scoped);
            container.Register<ArchivesDbContext>(Lifestyle.Scoped);

            container.Register<DbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);
            container.Register<IOjsDbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);
            container.Register<IArchivesDbContext>(container.GetInstance<ArchivesDbContext>, Lifestyle.Scoped);

            container.Register(
                typeof(IRepository<>),
                typeof(EfGenericRepository<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IEfGenericRepository<>),
                typeof(EfGenericRepository<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IArchivesGenericRepository<>),
                typeof(ArchivesGenericReposity<>),
                Lifestyle.Scoped);

            container.Register(
                typeof(IEfDeletableEntityRepository<>),
                typeof(EfDeletableEntityRepository<>),
                Lifestyle.Scoped);
        }
    }
}