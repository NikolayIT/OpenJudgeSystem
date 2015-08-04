namespace OJS.Web
{
    using System.Web.Mvc;

    using Ninject.Modules;
    using Ninject.Web.Mvc.FilterBindingSyntax;

    using OJS.Data;
    using OJS.Web.Common.Attributes;

    public class LoggingModule : NinjectModule
    {
        public override void Load()
        {
            this.BindFilter<LoggerFilterAttribute>(FilterScope.Controller, 0).WhenControllerHas<LogAttribute>();
            this.BindFilter<LoggerFilterAttribute>(FilterScope.Action, 0).WhenActionMethodHas<LogAttribute>();
        }
    }
}