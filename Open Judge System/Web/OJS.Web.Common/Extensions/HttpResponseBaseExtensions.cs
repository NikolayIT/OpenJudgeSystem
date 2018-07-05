namespace OJS.Web.Common.Extensions
{
    using System.Net;
    using System.Web;

    public static class HttpResponseBaseExtensions
    {
        public static bool IsError(this HttpResponseBase response) =>
            response.StatusCode >= (int)HttpStatusCode.BadRequest;
    }
}