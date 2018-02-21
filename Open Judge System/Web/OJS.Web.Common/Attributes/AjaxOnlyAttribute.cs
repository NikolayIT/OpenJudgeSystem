namespace OJS.Web.Common.Attributes
{
    using System.Reflection;
    using System.Web.Mvc;

    public class AjaxOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo) =>
            controllerContext.RequestContext.HttpContext.Request.IsAjaxRequest();
    }
}