namespace OJS.Web
{
    using Glimpse.AspNet.Extensions;
    using Glimpse.Core.Extensibility;

    using OJS.Web.Common.Extensions;

    public class GlimpseSecurityPolicy : IRuntimePolicy
    {
        public RuntimeEvent ExecuteOn => RuntimeEvent.EndRequest | RuntimeEvent.ExecuteResource;

        public RuntimePolicy Execute(IRuntimePolicyContext policyContext)
        {
            // More information about RuntimePolicies can be found at http://getglimpse.com/Help/Custom-Runtime-Policy
            var httpContext = policyContext.GetHttpContext();
            return httpContext.User.IsAdmin() ? RuntimePolicy.On : RuntimePolicy.Off;
        }
    }
}
