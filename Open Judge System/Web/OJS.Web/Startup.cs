[assembly: Microsoft.Owin.OwinStartup(typeof(OJS.Web.Startup))]

namespace OJS.Web
{
    using Owin;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
        }
    }
}
