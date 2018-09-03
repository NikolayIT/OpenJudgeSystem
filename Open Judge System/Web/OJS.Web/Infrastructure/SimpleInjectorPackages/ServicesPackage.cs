namespace OJS.Web.Infrastructure.SimpleInjectorPackages
{
    using System.Linq;

    using MissingFeatures;

    using OJS.Services.Business.ExamGroups;
    using OJS.Services.Common;
    using OJS.Services.Common.BackgroundJobs;
    using OJS.Services.Common.HttpRequester;
    using OJS.Services.Data.ExamGroups;
    using OJS.Services.Data.SubmissionsForProcessing;
    using OJS.Services.Data.Users;

    using SimpleInjector;
    using SimpleInjector.Packaging;

    public class ServicesPackage : IPackage
    {
        public void RegisterServices(Container container)
        {
            this.RegisterCustomTypes(container);

            this.RegisterNonGenericTypes(container);
        }

        private void RegisterCustomTypes(Container container)
        {
            container.Register<IExamGroupsBusinessService>(
                () => new ExamGroupsBusinessService(
                    container.GetInstance<IExamGroupsDataService>(),
                    container.GetInstance<IUsersDataService>(),
                    container.GetInstance<IHttpRequesterService>(),
                    container.GetInstance<IHangfireBackgroundJobService>(),
                    Settings.SulsPlatformBaseUrl,
                    Settings.ApiKey),
                Lifestyle.Scoped);
        }

        private void RegisterNonGenericTypes(Container container)
        {
            var serviceAssemblies = new[]
            {
                typeof(ISubmissionsForProcessingDataService).Assembly,
                typeof(IHangfireBackgroundJobService).Assembly
            };

            var registrations = serviceAssemblies
                .SelectMany(a => a.GetExportedTypes())
                .Where(type =>
                    typeof(IService).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsGenericTypeDefinition)
                .Select(type => new
                {
                    ServiceTypes = type
                        .GetInterfaces()
                        .Where(i =>
                            i.IsPublic &&
                            !i.GenericTypeArguments.Any() &&
                            i != typeof(IService)),
                    Implementation = type
                });

            foreach (var registration in registrations)
            {
                registration.ServiceTypes.ForEach(
                    service => container.Register(service, registration.Implementation, Lifestyle.Scoped));
            }
        }
    }
}