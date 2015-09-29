namespace OJS.Tests.Common.WebStubs
{
    using System.Collections.Specialized;
    using System.Web;

    public class StubHttpRequestForRouting : HttpRequestBase
    {
        public StubHttpRequestForRouting(string appPath, string requestUrl)
        {
            this.ApplicationPath = appPath;
            this.AppRelativeCurrentExecutionFilePath = requestUrl;
        }

        public override string ApplicationPath { get; }

        public override string AppRelativeCurrentExecutionFilePath { get; }

        public override string PathInfo => string.Empty;

        public override NameValueCollection ServerVariables => new NameValueCollection();
    }
}
