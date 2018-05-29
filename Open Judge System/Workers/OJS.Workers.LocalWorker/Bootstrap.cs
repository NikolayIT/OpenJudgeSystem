namespace OJS.Workers.LocalWorker
{
    using System.Data.Entity;

    using OJS.Data;
    using OJS.Data.Repositories.Base;
    using OJS.Data.Repositories.Contracts;
    using OJS.Services.Business.SubmissionsForProcessing;
    using OJS.Services.Data.Participants;
    using OJS.Services.Data.ParticipantScores;
    using OJS.Services.Data.SubmissionsForProcessing;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class Bootstrap
    {
        public static Container Container;

        public static void Start(Container container)
        {
            container.Options.DefaultScopedLifestyle = new ThreadScopedLifestyle();

            container.Register<LocalWorkerService>(Lifestyle.Scoped);
            container.Register<OjsDbContext>(Lifestyle.Scoped);

            container.Register<DbContext>(container.GetInstance<OjsDbContext>, Lifestyle.Scoped);
            container.Register<IOjsDbContext, OjsDbContext>(Lifestyle.Scoped);
            container.Register(typeof(IEfGenericRepository<>), typeof(EfGenericRepository<>), Lifestyle.Scoped);
           
            container.Register<
                ISubmissionsForProcessingDataService,
                SubmissionsForProcessingDataService>(Lifestyle.Scoped);

            container.Register<
                ISubmissionsForProcessingBusinessService,
                SubmissionsForProcessingBusinessService>(Lifestyle.Scoped);

            container.Register<
                IParticipantsDataService,
                ParticipantsDataService>(Lifestyle.Scoped);

            container.Register<
                IParticipantScoresDataService,
                ParticipantScoresDataService>(Lifestyle.Scoped);

            container.Verify();

            Container = container;
        }
    }
}