namespace OJS.Web.HangfireConfiguration
{
    using System.Web.Hosting;

    using Hangfire;

    public class HangfireBootstrapper : IRegisteredObject
    {
        private static readonly object LockObject = new object();

        public static HangfireBootstrapper instance;
   
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
                   //GlobalConfiguration.Configuration.UseNinjectActivator(new Ninject.Web.Common.Bootstrapper().Kernel);
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