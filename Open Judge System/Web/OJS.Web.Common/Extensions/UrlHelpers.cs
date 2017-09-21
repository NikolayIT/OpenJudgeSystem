namespace OJS.Web.Common.Extensions
{
    using System;
    using System.Linq;

    public static class UrlHelpers
    {
        public static string ExtractFullContestsTreeUrlFromPath(string path)
        {
            var urlSegments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var leftPart = $"/{urlSegments[0]}/";
            var righPart = string.Join("/", urlSegments.Skip(1)).TrimEnd('/');
            var newUrl = $"{leftPart}#!/{righPart}";
            return newUrl;
        }
    }
}