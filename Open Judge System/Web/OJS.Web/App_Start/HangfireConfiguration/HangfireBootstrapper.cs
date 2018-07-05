namespace OJS.Web.HangfireConfiguration
{
    using System.Web.Hosting;

    using Hangfire;
    using Hangfire.SimpleInjector;

    public class HangfireBootstrapper : IRegisteredObject
    {
        private static readonly object LockObject = new object();

        private static HangfireBootstrapper instance;

        private bool started;
        private BackgroundJobServer backgroundJobServer;

        private HangfireBootstrapper()
        {
        }

        public static HangfireBootstrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (LockObject)
                    {
                        if (instance == null)
                        {
                            instance = new HangfireBootstrapper();
                        }
                    }
                }

                return instance;
            }
        }

        public void Start()
        {
            lock (LockObject)
            {
                if (!this.started)
                {
                    this.started = true;

                    HostingEnvironment.RegisterObject(this);
                    GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
                    GlobalConfiguration.Configuration
                        .UseActivator(new SimpleInjectorJobActivator(SimpleInjectorConfig.Container));
                    this.backgroundJobServer = new BackgroundJobServer();
                }
            }
        }

        public void Stop()
        {
            lock (LockObject)
            {
                this.started = false;

                this.backgroundJobServer?.Dispose();

                HostingEnvironment.UnregisterObject(this);
            }
        }

        void IRegisteredObject.Stop(bool immediate)
        {
            this.Stop();
        }
    }
}