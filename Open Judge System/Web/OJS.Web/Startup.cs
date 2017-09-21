[assembly: Microsoft.Owin.OwinStartup(typeof(OJS.Web.Startup))]

namespace OJS.Web
{
    using Hangfire;

    using OJS.Web.HangfireConfiguration;

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

            var options = new DashboardOptions
            {
                Authorization = new[] { new HangFireAuthenticationFilter(), }
            };
            app.UseHangfireDashboard("/hangfire", options);
        }
    }
}
