namespace OJS.Web.Common.Attributes
{
    using System.Web;
    using System.Web.Mvc;

    public class DisableCache : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            var response = filterContext.HttpContext.Response;

            response.Cache.SetCacheability(HttpCacheability.NoCache);
            response.Cache.AppendCacheExtension("no-store, must-revalidate");
            response.AppendHeader("Pragma", "no-cache");
            response.AppendHeader("Expires", "0");
        }
    }
}