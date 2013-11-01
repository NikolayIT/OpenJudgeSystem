namespace OJS.Web.Areas.Administration.Controllers
{
    using System.Web.Mvc;

    using OJS.Data;
    using OJS.Web.Controllers;

    public class NavigationController : AdministrationController
    {
        public NavigationController(IOjsData data)
            : base(data)
        {
        }

        public ActionResult Index()
        {
            return this.View();
        }
    }
}