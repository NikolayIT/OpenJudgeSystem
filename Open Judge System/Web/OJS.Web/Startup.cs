[assembly: Microsoft.Owin.OwinStartup(typeof(OJS.Web.Startup))]

namespace OJS.Web
{
    using Hangfire;

    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
            this.ConfigureHangfire(app);
        }

        private void ConfigureHangfire(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
