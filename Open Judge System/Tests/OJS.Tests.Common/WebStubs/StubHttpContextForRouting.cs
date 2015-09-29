namespace OJS.Tests.Common.WebStubs
{
    using System.Web;

    public class StubHttpContextForRouting : HttpContextBase
    {
        private readonly StubHttpRequestForRouting request;

        private readonly StubHttpResponseForRouting response;

        public StubHttpContextForRouting(string appPath = "/", string requestUrl = "~/")
        {
            this.request = new StubHttpRequestForRouting(appPath, requestUrl);
            this.response = new StubHttpResponseForRouting();
        }

        public override HttpRequestBase Request => this.request;

        public override HttpResponseBase Response => this.response;
    }
}
