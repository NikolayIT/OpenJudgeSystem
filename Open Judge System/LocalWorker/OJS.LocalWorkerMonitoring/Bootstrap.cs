namespace OJS.LocalWorkerMonitoring
{
    using log4net;

    using OJS.Services.Common.Emails;
    using OJS.Workers.Common;

    using SimpleInjector;
    using SimpleInjector.Lifestyles;

    internal class Bootstrap
    {
        public static Container Container;

        public static void Start(Container container)
        {
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            RegisterTypes(container);

            container.Verify();

            Container = container;
        }

        private static void RegisterTypes(Container container)
        {
            container.Register(() =>
                new LocalWorkerMonitoringService(
                    container.GetInstance<ILog>(),
                    container.GetInstance<IEmailSenderService>(),
                    Settings.DevEmail),
                Lifestyle.Scoped);

            container.RegisterInstance<ILog>(
                LogManager.GetLogger(Constants.LocalWorkerMonitoringServiceLogName));

            container.Register<IEmailSenderService>(
                () => new EmailSenderService(
                    Settings.EmailServerHost,
                    Settings.EmailServerPort,
                    Settings.EmailServerUsername,
                    Settings.EmailServerPassword,
                    Settings.EmailSenderEmail,
                    Settings.EmailSenderDisplayName),
                Lifestyle.Scoped);
        }
    }
}