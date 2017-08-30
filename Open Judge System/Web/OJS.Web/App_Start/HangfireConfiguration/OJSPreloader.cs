namespace OJS.Web.HangFireConfiguration
{
    using System.Web.Hosting;

    using HangfireConfiguration;

    public class OjsPreloader : IProcessHostPreloadClient
    {
        public void Preload(string[] parameters)
        {
            HangfireBootstrapper.Instance.Start();
        }
    }
}