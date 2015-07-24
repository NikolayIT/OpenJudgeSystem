namespace OJS.Web
{
    using System.Web.Mvc;

    public class ViewEngineConfig
    {
        public static void RegisterViewEngines(ViewEngineCollection viewEngines)
        {
            viewEngines.Clear();

            viewEngines.Add(new RazorViewEngine());
        }
    }
}