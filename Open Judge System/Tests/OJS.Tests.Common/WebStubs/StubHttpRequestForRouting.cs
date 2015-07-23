namespace OJS.Tests.Common.WebStubs
{
    using System.Collections.Specialized;
    using System.Web;

    public class StubHttpRequestForRouting : HttpRequestBase
    {
        private readonly string appPath;

        private readonly string requestUrl;

        public StubHttpRequestForRouting(string appPath, string requestUrl)
        {
            this.appPath = appPath;
            this.requestUrl = requestUrl;
        }

        public override string ApplicationPath
        {
            get
            {
                return this.appPath;
            }
        }

        public override string AppRelativeCurrentExecutionFilePath
        {
            get
            {
                return this.requestUrl;
            }
        }

        public override string PathInfo
        {
            get
            {
                return string.Empty;
            }
        }

        public override NameValueCollection ServerVariables
        {
            get
            {
                return new NameValueCollection();
            }
        }
    }
}
